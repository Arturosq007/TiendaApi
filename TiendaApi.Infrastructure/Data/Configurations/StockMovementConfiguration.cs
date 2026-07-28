using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;
public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // ─── Propiedades ──────────────────────────────────────────────────────

        builder.Property(sm => sm.Date)
            .IsRequired();

        builder.Property(sm => sm.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.StockBefore)
            .IsRequired();

        builder.Property(sm => sm.StockAfter)
            .IsRequired();

        builder.Property(sm => sm.Reason)
            .HasMaxLength(500);

        builder.Property(sm => sm.ReferenceType)
            .HasMaxLength(20);

        // ─── Índices ──────────────────────────────────────────────────────────

        // Consulta más frecuente: historial de un producto específico
        builder.HasIndex(sm => sm.ProductId)
            .HasDatabaseName("IX_StockMovements_ProductId");

        // Filtros por fecha en reportes
        builder.HasIndex(sm => sm.Date)
            .HasDatabaseName("IX_StockMovements_Date");

        // Índice compuesto para reportes por producto y fecha
        builder.HasIndex(sm => new { sm.ProductId, sm.Date })
            .HasDatabaseName("IX_StockMovements_ProductId_Date");

        // ─── Relaciones ───────────────────────────────────────────────────────

        builder.HasOne(sm => sm.Product)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.User)
            .WithMany(u => u.StockMovements)
            .HasForeignKey(sm => sm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}