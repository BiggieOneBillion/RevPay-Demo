using Microsoft.EntityFrameworkCore;
using RevPay.Domain.Enums;
using RevPay.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Jobs;

public class OverdueBillsJob
{
    private readonly AppDbContext _db;

    public OverdueBillsJob(AppDbContext db)
    {
        _db = db;
    }

    public async Task MarkOverdueBillsAsync()
    {
        var now = DateTime.UtcNow;

        var overdueBills = await _db.Bills
            .Where(b => b.Status == BillStatus.Pending 
                && b.DueDate < now)
            .ToListAsync();

        foreach (var bill in overdueBills)
        {
            // Update status to overdue
            // Note: In a real app, you might want to use a domain method to ensure logic is encapsulated
            var statusProperty = typeof(RevPay.Domain.Entities.Bill).GetProperty("Status");
            statusProperty?.SetValue(bill, BillStatus.Overdue);
        }

        await _db.SaveChangesAsync();
    }
}
