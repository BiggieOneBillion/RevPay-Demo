using Microsoft.EntityFrameworkCore;
using RevPay.Domain.Common;
using RevPay.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Taxpayer> Taxpayers => Set<Taxpayer>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<RevPay.Domain.Entities.Payment> Payments => Set<RevPay.Domain.Entities.Payment>();
    public DbSet<MDA> Mdas => Set<MDA>();
    public DbSet<RevenueHead> RevenueHeads => Set<RevenueHead>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReconciliationReport> ReconciliationReports => Set<ReconciliationReport>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AppRefreshToken> RefreshTokens => Set<AppRefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.HasDefaultSchema("revpay");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        var result = await base.SaveChangesAsync(ct);
        return result;
    }
}
