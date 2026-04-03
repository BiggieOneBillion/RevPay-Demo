using RevPay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Application.Common.Interfaces;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<GatewayInitResult> InitializeAsync(GatewayInitRequest request, CancellationToken ct);
    Task<GatewayVerifyResult> VerifyAsync(string reference, CancellationToken ct);
    Task<GatewayRefundResult> RefundAsync(string reference, decimal amount, CancellationToken ct);
}

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(string providerName);
}

public interface ILedgerService
{
    Task PostPaymentEntriesAsync(Payment payment, List<Bill> bills, CancellationToken ct);
}

public interface IReceiptService
{
    Task<Receipt> GenerateAsync(Guid paymentId, CancellationToken ct);
}

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}

public interface ISmsService
{
    Task SendAsync(SmsMessage message, CancellationToken ct);
}

public interface INotificationService
{
    Task SendReconciliationAlertAsync(string provider, DateTime reportDate, decimal variance);
}

public interface IIdempotencyService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class;
}

public interface IWebhookValidator
{
    Task<bool> ValidateAsync(string provider, object payload, string signature);
}

public interface IJwtService
{
    string GenerateAccessToken(UserClaims claims);
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);
}

// Result Models
public record GatewayInitRequest { public string Reference { get; init; } public decimal Amount { get; init; } public string Email { get; init; } public object Metadata { get; init; } }
public record GatewayInitResult { public bool IsSuccess { get; init; } public string AuthorizationUrl { get; init; } public string AccessCode { get; init; } public string Reference { get; init; } }
public record GatewayVerifyResult { public bool IsSuccessful { get; init; } public string MerchantReference { get; init; } public string GatewayReference { get; init; } public string? AuthorizationCode { get; init; } public string Channel { get; init; } public DateTime PaidAt { get; init; } }
public record GatewayRefundResult { public bool IsSuccessful { get; init; } }
public record EmailMessage { public string To { get; init; } public string Subject { get; init; } public string TemplateName { get; init; } public object TemplateData { get; init; } }
public record SmsMessage { public string To { get; init; } public string Body { get; init; } }
public record UserClaims { public Guid UserId { get; init; } public string Email { get; init; } public Guid? TaxpayerId { get; init; } public string Role { get; init; } public Guid? MdaId { get; init; } }
public record RefreshToken { public Guid UserId { get; init; } public string TokenHash { get; init; } public string RawToken { get; init; } public DateTime ExpiresAt { get; init; } public string CreatedByIp { get; init; } }
