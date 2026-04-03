using RevPay.Domain.Common;
using RevPay.Domain.Enums;
using System;

namespace RevPay.Domain.Entities;

public class LedgerEntry : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid? BillId { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public string AccountCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime EntryDate { get; set; }
}

public class Receipt : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; }
    public DateTime IssuedAt { get; set; }
    public string PdfUrl { get; set; }

    public static Receipt Create(Guid paymentId, string receiptNumber, string pdfUrl)
    {
        return new Receipt
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            ReceiptNumber = receiptNumber,
            PdfUrl = pdfUrl,
            IssuedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
}

public class ReconciliationReport : AggregateRoot<Guid>
{
    public DateTime ReportDate { get; private set; }
    public string GatewayProvider { get; private set; }
    public decimal InternalTotal { get; private set; }
    public decimal GatewayTotal { get; private set; }
    public decimal Variance { get; private set; }
    public string Status { get; private set; }

    private ReconciliationReport() { }

    public static ReconciliationReport Create(DateTime reportDate, string provider, decimal internalTotal, decimal gatewayTotal, decimal variance)
    {
        return new ReconciliationReport
        {
            Id = Guid.NewGuid(),
            ReportDate = reportDate,
            GatewayProvider = provider,
            InternalTotal = internalTotal,
            GatewayTotal = gatewayTotal,
            Variance = variance,
            Status = Math.Abs(variance) < 0.01m ? "Balanced" : "Variance",
            CreatedAt = DateTime.UtcNow
        };
    }
}
