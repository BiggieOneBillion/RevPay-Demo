using Microsoft.Extensions.Logging;
using RevPay.Application.Common.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Payments.Gateways;

public class FlutterwaveGateway : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly ILogger<FlutterwaveGateway> _logger;

    public string ProviderName => "Flutterwave";

    public FlutterwaveGateway(HttpClient http, ILogger<FlutterwaveGateway> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<GatewayInitResult> InitializeAsync(GatewayInitRequest request, CancellationToken ct)
    {
        var payload = new
        {
            tx_ref = request.Reference,
            amount = request.Amount,
            currency = "NGN",
            redirect_url = "https://revpay.lagosstate.gov.ng/payments/callback",
            customer = new { email = request.Email },
            meta = request.Metadata
        };

        _logger.LogInformation("Initializing Flutterwave transaction for reference {Ref}", request.Reference);
        var response = await _http.PostAsJsonAsync("payments", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<FlwInitResponse>(cancellationToken: ct);

        return new GatewayInitResult
        {
            IsSuccess = json!.Status == "success",
            AuthorizationUrl = json.Data.Link,
            AccessCode = request.Reference,
            Reference = request.Reference
        };
    }

    public async Task<GatewayVerifyResult> VerifyAsync(string reference, CancellationToken ct)
    {
        var response = await _http.GetAsync($"transactions/{reference}/verify", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<FlwVerifyResponse>(cancellationToken: ct);
        var data = json!.Data;

        return new GatewayVerifyResult
        {
            IsSuccessful = data.Status == "successful",
            MerchantReference = data.TxRef,
            GatewayReference = data.Id.ToString(),
            AuthorizationCode = data.Id.ToString(),
            Channel = data.PaymentType ?? "card",
            PaidAt = data.CreatedAt
        };
    }

    public async Task<GatewayRefundResult> RefundAsync(string reference, decimal amount, CancellationToken ct)
    {
        var payload = new { amount };
        var response = await _http.PostAsJsonAsync($"transactions/{reference}/refund", payload, ct);
        return new GatewayRefundResult { IsSuccessful = response.IsSuccessStatusCode };
    }
}

// ─── Flutterwave response DTOs ────────────────────────────────────────────────

file class FlwInitResponse
{
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("data")] public FlwInitData Data { get; set; } = default!;
}
file class FlwInitData
{
    [JsonPropertyName("link")] public string Link { get; set; } = "";
}
file class FlwVerifyResponse
{
    [JsonPropertyName("data")] public FlwVerifyData Data { get; set; } = default!;
}
file class FlwVerifyData
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("tx_ref")] public string TxRef { get; set; } = "";
    [JsonPropertyName("payment_type")] public string? PaymentType { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
}
