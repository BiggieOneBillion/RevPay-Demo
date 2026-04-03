using Microsoft.Extensions.Logging;
using RevPay.Application.Common.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RevPay.Infrastructure.Payments;

public class WebhookValidator : IWebhookValidator
{
    private readonly ILogger<WebhookValidator> _logger;

    public WebhookValidator(ILogger<WebhookValidator> logger) => _logger = logger;

    public Task<bool> ValidateAsync(string provider, object payload, string signature)
    {
        // Signature secret is injected per-provider from configuration.
        // This method is called from the controller with the raw body + secret.
        // The actual validation is done in the typed helpers below.
        _logger.LogWarning("WebhookValidator.ValidateAsync called without a secret key. " +
                           "Use ValidatePaystack or ValidateFlutterwave directly.");
        return Task.FromResult(false);
    }

    /// <summary>Validates Paystack webhook using HMAC-SHA512.</summary>
    public static bool ValidatePaystack(string rawBody, string signature, string secretKey)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    /// <summary>Validates Flutterwave webhook using HMAC-SHA256.</summary>
    public static bool ValidateFlutterwave(string rawBody, string signature, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }
}
