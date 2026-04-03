using MediatR;
using RevPay.Application.Common.Exceptions;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Entities;
using RevPay.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Application.Bills.Queries;

// ─── Shared DTOs ─────────────────────────────────────────────────────────────

public record BillSummaryDto(Guid Id, string BillNumber, string Description,
    decimal Amount, decimal Penalty, decimal TotalAmount,
    BillStatus Status, DateTime DueDate);

public record BillDetailDto(Guid Id, string BillNumber, string Description,
    decimal Amount, decimal Penalty, decimal TotalAmount,
    BillStatus Status, DateTime IssueDate, DateTime DueDate,
    DateTime? PaidDate, string? PaymentReference, int AssessmentYear,
    Guid MdaId, string RevenueHeadCode);

public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

// ─── GetTaxpayerBills ─────────────────────────────────────────────────────────

public record GetTaxpayerBillsQuery(
    Guid TaxpayerId,
    BillStatus? Status,
    Guid? MdaId,
    int Page,
    int PageSize
) : IRequest<PagedResult<BillSummaryDto>>;

public class GetTaxpayerBillsHandler : IRequestHandler<GetTaxpayerBillsQuery, PagedResult<BillSummaryDto>>
{
    private readonly IBillRepository _bills;
    public GetTaxpayerBillsHandler(IBillRepository bills) => _bills = bills;

    public async Task<PagedResult<BillSummaryDto>> Handle(GetTaxpayerBillsQuery q, CancellationToken ct)
    {
        var (items, total) = await _bills.GetPagedAsync(q.TaxpayerId, q.Status, q.MdaId, q.Page, q.PageSize, ct);
        var dtos = items.Select(b => new BillSummaryDto(
            b.Id, b.BillNumber, b.Description, b.Amount, b.PenaltyAmount,
            b.TotalAmount, b.Status, b.DueDate)).ToList();
        return new PagedResult<BillSummaryDto>(dtos, total, q.Page, q.PageSize);
    }
}

// ─── GetBillByNumber ──────────────────────────────────────────────────────────

public record GetBillByNumberQuery(string BillNumber, Guid TaxpayerId) : IRequest<BillDetailDto>;

public class GetBillByNumberHandler : IRequestHandler<GetBillByNumberQuery, BillDetailDto>
{
    private readonly IBillRepository _bills;
    public GetBillByNumberHandler(IBillRepository bills) => _bills = bills;

    public async Task<BillDetailDto> Handle(GetBillByNumberQuery q, CancellationToken ct)
    {
        var bill = await _bills.GetByBillNumberAsync(q.BillNumber, ct)
            ?? throw new NotFoundException($"Bill '{q.BillNumber}' not found.");

        if (bill.TaxpayerId != q.TaxpayerId)
            throw new ForbiddenException("You don't have access to this bill.");

        return new BillDetailDto(bill.Id, bill.BillNumber, bill.Description,
            bill.Amount, bill.PenaltyAmount, bill.TotalAmount, bill.Status,
            bill.IssueDate, bill.DueDate, bill.PaidDate, bill.PaymentReference,
            bill.AssessmentYear, bill.MdaId, bill.RevenueHeadCode);
    }
}

// ─── CreateBill ───────────────────────────────────────────────────────────────

public record CreateBillCommand(
    Guid TaxpayerId, Guid MdaId, Guid RevenueHeadId,
    string RevenueHeadCode, string Description,
    decimal Amount, DateTime DueDate, int AssessmentYear
) : IRequest<BillDetailDto>;

public class CreateBillHandler : IRequestHandler<CreateBillCommand, BillDetailDto>
{
    private readonly IBillRepository _bills;
    private readonly IUnitOfWork _uow;

    public CreateBillHandler(IBillRepository bills, IUnitOfWork uow)
    { _bills = bills; _uow = uow; }

    public async Task<BillDetailDto> Handle(CreateBillCommand cmd, CancellationToken ct)
    {
        // Generate sequential bill number: LASG-YYYY-XXXXXXXX
        var billNumber = $"LASG-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var bill = Bill.Create(billNumber, cmd.TaxpayerId, cmd.MdaId, cmd.RevenueHeadId,
            cmd.RevenueHeadCode, cmd.Description, cmd.Amount, cmd.DueDate, cmd.AssessmentYear);

        await _bills.AddAsync(bill, ct);
        await _uow.SaveChangesAsync(ct);

        return new BillDetailDto(bill.Id, bill.BillNumber, bill.Description,
            bill.Amount, bill.PenaltyAmount, bill.TotalAmount, bill.Status,
            bill.IssueDate, bill.DueDate, bill.PaidDate, bill.PaymentReference,
            bill.AssessmentYear, bill.MdaId, bill.RevenueHeadCode);
    }
}
