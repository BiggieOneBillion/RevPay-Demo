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

namespace RevPay.Application.Payments.Commands;

public record InitiatePaymentCommand(
    Guid TaxpayerId,
    List<Guid> BillIds,
    PaymentChannel Channel,
    string GatewayProvider,
    string IdempotencyKey
) : IRequest<InitiatePaymentResult>;

public record InitiatePaymentResult(
    Guid PaymentId,
    string PaymentReference,
    string GatewayAuthorizationUrl,
    decimal TotalAmount
);

public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    private readonly IPaymentRepository _payments;
    private readonly IBillRepository _bills;
    private readonly ITaxpayerRepository _taxpayers;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IIdempotencyService _idempotency;
    private readonly IUnitOfWork _uow;

    public InitiatePaymentHandler(
        IPaymentRepository payments,
        IBillRepository bills,
        ITaxpayerRepository taxpayers,
        IPaymentGatewayFactory gatewayFactory,
        IIdempotencyService idempotency,
        IUnitOfWork uow)
    {
        _payments = payments;
        _bills = bills;
        _taxpayers = taxpayers;
        _gatewayFactory = gatewayFactory;
        _idempotency = idempotency;
        _uow = uow;
    }

    public async Task<InitiatePaymentResult> Handle(
        InitiatePaymentCommand cmd, CancellationToken ct)
    {
        // 1. Idempotency check 
        var cached = await _idempotency.GetAsync<InitiatePaymentResult>(cmd.IdempotencyKey);
        if (cached is not null) return cached;

        // 2. Load and validate bills
        var bills = await _bills.GetByIdsAsync(cmd.BillIds, ct);
        if (!bills.Any()) throw new NotFoundException("No bills found for the provided IDs.");
        
        var taxpayer = await _taxpayers.GetByIdAsync(cmd.TaxpayerId, ct)
            ?? throw new NotFoundException("Taxpayer", cmd.TaxpayerId);

        ValidateBills(bills, cmd.TaxpayerId);

        var totalAmount = bills.Sum(b => b.TotalAmount);

        // 3. Create payment aggregate
        var payment = Payment.Create(
            cmd.TaxpayerId, taxpayer.Email, totalAmount,
            cmd.Channel, cmd.GatewayProvider,
            cmd.IdempotencyKey);

        foreach (var bill in bills)
            payment.AddBill(bill.Id, bill.TotalAmount);

        // 4. Call payment gateway
        var gateway = _gatewayFactory.GetGateway(cmd.GatewayProvider);
        var gatewayResult = await gateway.InitializeAsync(new GatewayInitRequest
        {
            Reference = payment.PaymentReference,
            Amount = totalAmount,
            Email = taxpayer.Email,
            Metadata = new { payment_id = payment.Id, bill_ids = cmd.BillIds }
        }, ct);

        if (!gatewayResult.IsSuccess)
            throw new BusinessRuleException("Failed to initialize payment with the gateway.");

        payment.SetGatewayInitResponse(gatewayResult.Reference);

        // 5. Persist
        await _payments.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        var result = new InitiatePaymentResult(
            payment.Id, payment.PaymentReference,
            gatewayResult.AuthorizationUrl, totalAmount);

        // 6. Cache for idempotency (24 hours)
        await _idempotency.SetAsync(cmd.IdempotencyKey, result, TimeSpan.FromHours(24));
        return result;
    }

    private static void ValidateBills(List<Bill> bills, Guid taxpayerId)
    {
        if (bills.Any(b => b.TaxpayerId != taxpayerId))
            throw new ForbiddenException("One or more bills do not belong to this taxpayer.");
        if (bills.Any(b => b.Status == BillStatus.Paid))
            throw new BusinessRuleException("One or more bills are already paid.");
        if (bills.Any(b => b.Status == BillStatus.Cancelled))
            throw new BusinessRuleException("Cannot pay a cancelled bill.");
    }
}
