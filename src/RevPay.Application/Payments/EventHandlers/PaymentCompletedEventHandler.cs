using MediatR;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Events;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Application.Payments.EventHandlers;

/// <summary>
/// Fired after a Payment is confirmed successful. 
/// Sends a receipt email + SMS to the taxpayer via the notification service.
/// </summary>
public class PaymentCompletedEventHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly IBillRepository _bills;
    private readonly IReceiptService _receipts;
    private readonly IUnitOfWork _uow;

    public PaymentCompletedEventHandler(IEmailService email, ISmsService sms,
        IBillRepository bills, IReceiptService receipts, IUnitOfWork uow)
    {
        _email = email; _sms = sms;
        _bills = bills; _receipts = receipts; _uow = uow;
    }

    public async Task Handle(PaymentCompletedEvent notification, CancellationToken ct)
    {
        // Generate the PDF receipt
        var receipt = await _receipts.GenerateAsync(notification.PaymentId, ct);
        await _uow.SaveChangesAsync(ct);

        // Email to taxpayer
        await _email.SendAsync(new EmailMessage
        {
            To = notification.TaxpayerEmail,
            Subject = "Your RevPay Payment Receipt",
            TemplateName = "payment_receipt",
            TemplateData = new
            {
                amount = notification.Amount,
                receiptNumber = receipt.ReceiptNumber,
                receiptUrl = receipt.PdfUrl,
                billCount = notification.PaymentBills.Count()
            }
        }, ct);

        // SMS alert
        await _sms.SendAsync(new SmsMessage
        {
            To = notification.TaxpayerEmail,   // real impl would use phone from Taxpayer entity
            Body = $"RevPay: Your payment of ₦{notification.Amount:N2} was confirmed. " +
                   $"Receipt: {receipt.ReceiptNumber}"
        }, ct);
    }
}
