using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RevPay.API.Models;
using RevPay.Application.Payments.Commands;
using RevPay.Domain.Enums;
using RevPay.Infrastructure.Payments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RevPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public PaymentsController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config = config;
    }

    /// <summary>Initiate a payment session for one or more bills.</summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(ApiResponse.Error("Idempotency-Key header is required."));

        var result = await _mediator.Send(new InitiatePaymentCommand(
            TaxpayerId: GetTaxpayerId(),
            BillIds: request.BillIds,
            Channel: request.Channel,
            GatewayProvider: request.GatewayProvider,
            IdempotencyKey: idempotencyKey));

        return Ok(ApiResponse<InitiatePaymentResult>.SuccessResponse(result));
    }

    /// <summary>Receive gateway webhook and verify the payment.
    /// The raw body is read and HMAC signature is validated before processing.</summary>
    [HttpPost("webhook/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(string provider)
    {
        // Read raw body for signature verification
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var signature = Request.Headers["x-paystack-signature"].ToString()
            ?? Request.Headers["verif-hash"].ToString()
            ?? "";

        // Validate signature based on provider
        var secretKey = provider.ToLower() switch
        {
            "paystack"     => _config["GatewaySettings:PaystackWebhookSecret"] ?? "",
            "flutterwave"  => _config["GatewaySettings:FlutterwaveWebhookSecret"] ?? "",
            _              => ""
        };

        var isValid = provider.ToLower() switch
        {
            "paystack"    => WebhookValidator.ValidatePaystack(rawBody, signature, secretKey),
            "flutterwave" => WebhookValidator.ValidateFlutterwave(rawBody, signature, secretKey),
            _             => false
        };

        if (!isValid)
            return Unauthorized(new { message = "Invalid webhook signature." });

        // Extract reference from payload
        string? reference = null;
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            reference = doc.RootElement
                .GetProperty("data")
                .GetProperty("reference")
                .GetString();
        }
        catch
        {
            return BadRequest(new { message = "Could not parse webhook payload." });
        }

        if (string.IsNullOrEmpty(reference))
            return BadRequest(new { message = "Missing reference in webhook payload." });

        await _mediator.Send(new VerifyPaymentCommand(reference, provider));
        return Ok();
    }

    private Guid GetTaxpayerId()
    {
        var id = User.FindFirstValue("taxpayer_id");
        return string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);
    }
}

public record InitiatePaymentRequest(
    List<Guid> BillIds,
    PaymentChannel Channel,
    string GatewayProvider);
