using Microsoft.Extensions.Logging;
using RevPay.Application.Common.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Payments.Gateways;

public class PaystackGateway : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly ILogger<PaystackGateway> _logger;

    public string ProviderName => "Paystack";

    public PaystackGateway(HttpClient http, ILogger<PaystackGateway> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<GatewayInitResult> InitializeAsync(GatewayInitRequest request, CancellationToken ct)
    {
        var payload = new
        {
            email = request.Email,
            amount = (long)(request.Amount * 100),   // Paystack uses kobo (smallest unit)
            reference = request.Reference,
            metadata = request.Metadata
        };

        _logger.LogInformation("Initializing Paystack transaction for reference {Ref}", request.Reference);

        var response = await _http.PostAsJsonAsync("transaction/initialize", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<PaystackInitResponse>(cancellationToken: ct);

        return new GatewayInitResult
        {
            IsSuccess = json!.Status,
            AuthorizationUrl = json.Data.AuthorizationUrl,
            AccessCode = json.Data.AccessCode,
            Reference = json.Data.Reference
        };
    }

    public async Task<GatewayVerifyResult> VerifyAsync(string reference, CancellationToken ct)
    {
        _logger.LogInformation("Verifying Paystack transaction {Ref}", reference);

        var response = await _http.GetAsync($"transaction/verify/{reference}", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<PaystackVerifyResponse>(cancellationToken: ct);

        return new GatewayVerifyResult
        {
            IsSuccessful = json!.Data.Status == "success",
            MerchantReference = json.Data.Reference,
            GatewayReference = json.Data.Id.ToString(),
            AuthorizationCode = json.Data.Authorization?.AuthorizationCode,
            Channel = json.Data.Channel ?? "card",
            PaidAt = json.Data.PaidAt ?? DateTime.UtcNow
        };
    }

    public async Task<GatewayRefundResult> RefundAsync(string reference, decimal amount, CancellationToken ct)
    {
        var payload = new { transaction = reference, amount = (long)(amount * 100) };
        var response = await _http.PostAsJsonAsync("refund", payload, ct);
        return new GatewayRefundResult { IsSuccessful = response.IsSuccessStatusCode };
    }
}

// ─── Paystack response DTOs ───────────────────────────────────────────────────

file class PaystackInitResponse
{
    [JsonPropertyName("status")] public bool Status { get; set; }
    [JsonPropertyName("data")] public PaystackInitData Data { get; set; } = default!;
}
file class PaystackInitData
{
    [JsonPropertyName("authorization_url")] public string AuthorizationUrl { get; set; } = "";
    [JsonPropertyName("access_code")] public string AccessCode { get; set; } = "";
    [JsonPropertyName("reference")] public string Reference { get; set; } = "";
}
file class PaystackVerifyResponse
{
    [JsonPropertyName("data")] public PaystackVerifyData Data { get; set; } = default!;
}
file class PaystackVerifyData
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("reference")] public string Reference { get; set; } = "";
    [JsonPropertyName("channel")] public string? Channel { get; set; }
    [JsonPropertyName("paid_at")] public DateTime? PaidAt { get; set; }
    [JsonPropertyName("authorization")] public PaystackAuthorization? Authorization { get; set; }
}
file class PaystackAuthorization
{
    [JsonPropertyName("authorization_code")] public string AuthorizationCode { get; set; } = "";
}
