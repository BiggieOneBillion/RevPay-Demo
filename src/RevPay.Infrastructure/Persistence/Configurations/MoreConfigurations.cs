using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RevPay.Domain.Entities;

namespace RevPay.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentReference)
            .HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.PaymentReference).IsUnique();

        builder.Property(x => x.Amount)
            .HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(x => x.Channel)
            .HasConversion<string>().HasMaxLength(30);

        builder.HasMany(x => x.PaymentBills)
            .WithOne()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TaxpayerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
    }
}

public class PaymentBillConfiguration : IEntityTypeConfiguration<PaymentBill>
{
    public void Configure(EntityTypeBuilder<PaymentBill> builder)
    {
        builder.ToTable("payment_bills");
        builder.HasKey(x => new { x.PaymentId, x.BillId });

        builder.Property(x => x.AmountApplied)
            .HasColumnType("numeric(18,2)").IsRequired();

        builder.HasOne<Bill>()
            .WithMany()
            .HasForeignKey(x => x.BillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MdaConfiguration : IEntityTypeConfiguration<MDA>
{
    public void Configure(EntityTypeBuilder<MDA> builder)
    {
        builder.ToTable("mdas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.BankAccount).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        
        builder.HasMany(x => x.RevenueHeads)
            .WithOne()
            .HasForeignKey(x => x.MdaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RevenueHeadConfiguration : IEntityTypeConfiguration<RevenueHead>
{
    public void Configure(EntityTypeBuilder<RevenueHead> builder)
    {
        builder.ToTable("revenue_heads");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.GlAccountCode).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.MdaId, x.Code }).IsUnique();
    }
}

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.AccountCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntryType).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.AccountCode);
        builder.HasIndex(x => x.EntryDate);
    }
}

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReceiptNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.HasIndex(x => x.PaymentId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UserId).HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
