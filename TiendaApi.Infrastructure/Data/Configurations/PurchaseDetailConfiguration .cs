using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;

public class PurchaseDetailConfiguration : IEntityTypeConfiguration<PurchaseDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
    {
        builder.ToTable("PurchaseDetails");

        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(pd => pd.Quantity)
            .IsRequired();

        builder.Property(pd => pd.UnitCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(pd => pd.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        // ─── Índices ──────────────────────────────────────────────────────────

        builder.HasIndex(pd => new { pd.PurchaseId, pd.ProductId })
            .HasDatabaseName("IX_PurchaseDetails_PurchaseId_ProductId");

        // ─── Relaciones ───────────────────────────────────────────────────────

        builder.HasOne(pd => pd.Product)
            .WithMany(p => p.PurchaseDetails)
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
