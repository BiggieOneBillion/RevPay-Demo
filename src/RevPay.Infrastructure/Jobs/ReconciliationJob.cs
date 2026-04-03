using Microsoft.EntityFrameworkCore;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Entities;
using RevPay.Domain.Enums;
using RevPay.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Jobs;

public class ReconciliationJob
{
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public ReconciliationJob(
        IPaymentGatewayFactory gatewayFactory,
        AppDbContext db,
        INotificationService notifications)
    {
        _gatewayFactory = gatewayFactory;
        _db = db;
        _notifications = notifications;
    }

    public async Task RunDailyReconciliationAsync(DateTime reportDate)
    {
        var gateways = new[] { "Paystack", "Flutterwave", "Interswitch" };

        foreach (var provider in gateways)
        {
            // 1. Get our internal records for the day
            var internalTotal = await _db.Payments
                .Where(p => p.GatewayProvider == provider
                    && p.Status == PaymentStatus.Successful
                    && p.CompletedAt.HasValue 
                    && p.CompletedAt.Value.Date == reportDate.Date)
                .SumAsync(p => p.Amount);

            // 2. Fetch settlement data from gateway (Mocked for now)
            // var gateway = _gatewayFactory.GetGateway(provider);
            // var settlement = await gateway.GetSettlementAsync(reportDate);
            decimal settlementTotal = internalTotal; // Mocking zero variance for now

            // 3. Compute variance
            var variance = internalTotal - settlementTotal;

            // 4. Save report
            var report = ReconciliationReport.Create(
                reportDate, provider, internalTotal,
                settlementTotal, variance);

            await _db.ReconciliationReports.AddAsync(report);

            // 5. Alert if variance exceeds threshold
            if (Math.Abs(variance) > 100m) // > 100 Naira variance triggers alert
            {
                await _notifications.SendReconciliationAlertAsync(
                    provider, reportDate, variance);
            }
        }

        await _db.SaveChangesAsync();
    }
}
