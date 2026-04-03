using Microsoft.Extensions.Logging;
using RevPay.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Notifications;

public class NotificationService : INotificationService, IEmailService, ISmsService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendReconciliationAlertAsync(string provider, DateTime reportDate, decimal variance)
    {
        _logger.LogWarning("RECONCILIATION ALERT: Provider {Provider} on {Date} has variance of {Variance}", 
            provider, reportDate.ToShortDateString(), variance);
        await Task.CompletedTask;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        _logger.LogInformation("Sending EMAIL to {To}: {Subject}", message.To, message.Subject);
        await Task.CompletedTask;
    }

    public async Task SendAsync(SmsMessage message, CancellationToken ct)
    {
        _logger.LogInformation("Sending SMS to {To}: {Body}", message.To, message.Body);
        await Task.CompletedTask;
    }
}
