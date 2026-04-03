using RevPay.Domain.Common;
using RevPay.Domain.Enums;
using System;

namespace RevPay.Domain.Entities;

public class Bill : AggregateRoot<Guid>
{
    public string BillNumber { get; private set; }   // e.g. LASG-2024-0001234
    public Guid TaxpayerId { get; private set; }
    public Guid MdaId { get; private set; }
    public Guid RevenueHeadId { get; private set; }
    public string RevenueHeadCode { get; private set; } // e.g. LUC-001, PAYE-002
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PenaltyAmount { get; private set; }
    public decimal TotalAmount => Amount + PenaltyAmount;
    public BillStatus Status { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidDate { get; private set; }
    public string? PaymentReference { get; private set; }
    public int AssessmentYear { get; private set; }

    private Bill() { }

    public static Bill Create(string billNumber, Guid taxpayerId, Guid mdaId, Guid revenueHeadId, 
        string revenueHeadCode, string description, decimal amount, DateTime dueDate, int assessmentYear)
    {
        return new Bill
        {
            Id = Guid.NewGuid(),
            BillNumber = billNumber,
            TaxpayerId = taxpayerId,
            MdaId = mdaId,
            RevenueHeadId = revenueHeadId,
            RevenueHeadCode = revenueHeadCode,
            Description = description,
            Amount = amount,
            Status = BillStatus.Pending,
            IssueDate = DateTime.UtcNow,
            DueDate = dueDate,
            AssessmentYear = assessmentYear,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsPaid(string paymentReference)
    {
        if (Status == BillStatus.Paid)
            throw new Exception("Bill is already paid.");

        Status = BillStatus.Paid;
        PaidDate = DateTime.UtcNow;
        PaymentReference = paymentReference;
        // AddDomainEvent(new BillPaidEvent(Id, TaxpayerId, TotalAmount));
    }

    public void ApplyPenalty(decimal penaltyAmount)
    {
        if (Status == BillStatus.Paid)
            throw new Exception("Cannot apply penalty to a paid bill.");

        PenaltyAmount += penaltyAmount;
        // AddDomainEvent(new PenaltyAppliedEvent(Id, penaltyAmount));
    }
}
