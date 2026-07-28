using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // ─── Propiedades ──────────────────────────────────────────────────────

        builder.Property(p => p.Date)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.IGV)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Total)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.InvoiceNumber)
            .HasMaxLength(50);

        builder.Property(p => p.PaymentType)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(20);


        // ─── Índices ──────────────────────────────────────────────────────────

        builder.HasIndex(p => p.Date)
            .HasDatabaseName("IX_Purchases_Date");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Purchases_Status");

        // ─── Relaciones ───────────────────────────────────────────────────────

        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Purchases)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Purchases)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Details)
            .WithOne(d => d.Purchase)
            .HasForeignKey(d => d.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}