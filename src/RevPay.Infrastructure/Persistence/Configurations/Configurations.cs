using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RevPay.Domain.Entities;

namespace RevPay.Infrastructure.Persistence.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("bills");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BillNumber)
            .HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.BillNumber).IsUnique();

        builder.Property(x => x.Amount)
            .HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.PenaltyAmount)
            .HasColumnType("numeric(18,2)").HasDefaultValue(0m);

        builder.Property(x => x.Status)
            .HasConversion<string>().HasMaxLength(30);

        builder.HasOne<Taxpayer>()
            .WithMany(t => t.Bills)
            .HasForeignKey(b => b.TaxpayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MDA>()
            .WithMany()
            .HasForeignKey(b => b.MdaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TaxpayerId);
        builder.HasIndex(x => new { x.MdaId, x.Status });
        builder.HasIndex(x => x.DueDate);
    }
}

public class TaxpayerConfiguration : IEntityTypeConfiguration<Taxpayer>
{
    public void Configure(EntityTypeBuilder<Taxpayer> builder)
    {
        builder.ToTable("taxpayers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.BVN).HasMaxLength(11);
        builder.Property(x => x.NIN).HasMaxLength(11);
        builder.Property(x => x.TIN).HasMaxLength(15);
        
        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Street).HasColumnName("Address_Street").HasMaxLength(250);
            a.Property(p => p.City).HasColumnName("Address_City").HasMaxLength(100);
            a.Property(p => p.State).HasColumnName("Address_State").HasMaxLength(100);
            a.Property(p => p.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
        });

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.BVN).IsUnique();
        builder.HasIndex(x => x.TIN).IsUnique();
    }
}
