using Microsoft.EntityFrameworkCore;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Entities;
using RevPay.Domain.Enums;
using RevPay.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Ledger;

public class LedgerService : ILedgerService
{
    private readonly AppDbContext _db;

    public LedgerService(AppDbContext db) => _db = db;

    public async Task PostPaymentEntriesAsync(
        Payment payment, List<Bill> bills, CancellationToken ct)
    {
        var entries = new List<LedgerEntry>();

        foreach (var bill in bills)
        {
            var revenueHead = await _db.RevenueHeads
                .FirstOrDefaultAsync(x => x.Id == bill.RevenueHeadId, ct);

            if (revenueHead == null) continue;

            // Debit: Collection Clearing Account
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                BillId = bill.Id,
                EntryType = LedgerEntryType.Debit,
                AccountCode = "1001-COLLECTION-CLEARING",
                Amount = bill.TotalAmount,
                Description = $"Bill {bill.BillNumber} - {bill.Description}",
                EntryDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            // Credit: MDA Revenue GL Account
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                BillId = bill.Id,
                EntryType = LedgerEntryType.Credit,
                AccountCode = revenueHead.GlAccountCode,
                Amount = bill.TotalAmount,
                Description = $"Revenue: {revenueHead.Name}",
                EntryDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.LedgerEntries.AddRangeAsync(entries, ct);
    }
}
