using MediatR;
using RevPay.Application.Common.Exceptions;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Application.Payments.Commands;

public record VerifyPaymentCommand(string GatewayReference, string Provider)
    : IRequest<VerifyPaymentResult>;

public record VerifyPaymentResult(bool IsSuccessful, string Message);

public class VerifyPaymentHandler : IRequestHandler<VerifyPaymentCommand, VerifyPaymentResult>
{
    private readonly IPaymentRepository _payments;
    private readonly IBillRepository _bills;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ILedgerService _ledger;
    private readonly IReceiptService _receipts;
    private readonly IUnitOfWork _uow;

    public VerifyPaymentHandler(
        IPaymentRepository payments,
        IBillRepository bills,
        IPaymentGatewayFactory gatewayFactory,
        ILedgerService ledger,
        IReceiptService receipts,
        IUnitOfWork uow)
    {
        _payments = payments;
        _bills = bills;
        _gatewayFactory = gatewayFactory;
        _ledger = ledger;
        _receipts = receipts;
        _uow = uow;
    }

    public async Task<VerifyPaymentResult> Handle(
        VerifyPaymentCommand cmd, CancellationToken ct)
    {
        // 1. Verify with gateway
        var gateway = _gatewayFactory.GetGateway(cmd.Provider);
        var verification = await gateway.VerifyAsync(cmd.GatewayReference, ct);

        if (!verification.IsSuccessful)
            return new VerifyPaymentResult(false, "Gateway verification failed.");

        // 2. Load payment by gateway reference
        var payment = await _payments
            .GetByReferenceAsync(verification.GatewayReference, ct)
            ?? throw new NotFoundException("Payment", verification.GatewayReference);

        // 3. Guard — already processed (idempotent webhook handling)
        if (payment.Status == PaymentStatus.Successful)
            return new VerifyPaymentResult(true, "Already processed.");

        // 4. Complete payment and mark bills as paid
        payment.Complete(verification.GatewayReference, verification.AuthorizationCode ?? "");

        var bills = await _bills.GetByPaymentIdAsync(payment.Id, ct);
        foreach (var bill in bills)
            bill.MarkAsPaid(payment.PaymentReference);

        // 5. Post double-entry ledger entries
        await _ledger.PostPaymentEntriesAsync(payment, bills, ct);

        // 6. Generate receipt
        await _receipts.GenerateAsync(payment.Id, ct);

        await _uow.SaveChangesAsync(ct);
        return new VerifyPaymentResult(true, "Payment verified and processed.");
    }
}
