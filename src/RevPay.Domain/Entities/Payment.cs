using RevPay.Domain.Common;
using RevPay.Domain.Enums;
using RevPay.Domain.Events;
using System;
using System.Collections.Generic;

namespace RevPay.Domain.Entities;

public class Payment : AggregateRoot<Guid>
{
    public string PaymentReference { get; private set; }  // REVPAY-20241201-XXXXXXXX
    public Guid TaxpayerId { get; private set; }
    public string TaxpayerEmail { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentChannel Channel { get; private set; }   // Card, Transfer, USSD
    public string GatewayProvider { get; private set; }   // Paystack, Flutterwave
    public string? GatewayReference { get; private set; } // Gateway's own txn ref
    public string? GatewayAuthCode { get; private set; }
    public DateTime InitiatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string IdempotencyKey { get; private set; }    // Prevents duplicate charges

    private readonly List<PaymentBill> _paymentBills = new();
    public IReadOnlyCollection<PaymentBill> PaymentBills => _paymentBills.AsReadOnly();

    private Payment() { }

    public static Payment Create(Guid taxpayerId, string email, decimal amount, 
        PaymentChannel channel, string gatewayProvider, string idempotencyKey)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            PaymentReference = $"REVPAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            TaxpayerId = taxpayerId,
            TaxpayerEmail = email,
            Amount = amount,
            Status = PaymentStatus.Pending,
            Channel = channel,
            GatewayProvider = gatewayProvider,
            IdempotencyKey = idempotencyKey,
            InitiatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddBill(Guid billId, decimal amount)
    {
        _paymentBills.Add(new PaymentBill(Id, billId, amount));
    }

    public void SetGatewayInitResponse(string? gatewayReference)
    {
        GatewayReference = gatewayReference;
        Status = PaymentStatus.Processing;
    }

    public void Complete(string gatewayReference, string authCode)
    {
        Status = PaymentStatus.Successful;
        GatewayReference = gatewayReference;
        GatewayAuthCode = authCode;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new PaymentCompletedEvent(Id, TaxpayerId, Amount, TaxpayerEmail, PaymentBills));
    }

    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        AddDomainEvent(new PaymentFailedEvent(Id, reason));
    }
}

public class PaymentBill
{
    public Guid PaymentId { get; private set; }
    public Guid BillId { get; private set; }
    public decimal AmountApplied { get; private set; }

    public PaymentBill(Guid paymentId, Guid billId, decimal amountApplied)
    {
        PaymentId = paymentId;
        BillId = billId;
        AmountApplied = amountApplied;
    }
}
