using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;
public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");

        builder.HasKey(sd => sd.Id);

        builder.Property(sd => sd.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(sd => sd.Quantity)
            .IsRequired();

        builder.Property(sd => sd.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sd => sd.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        // Índice compuesto — acelera consultas del tipo
        // "dame todos los detalles de esta venta"
        builder.HasIndex(sd => new { sd.SaleId, sd.ProductId })
            .HasDatabaseName("IX_SaleDetails_SaleId_ProductId");

        // Relación con Product — Restrict porque no querés
        // que se elimine un producto que tiene historial de ventas
        builder.HasOne(sd => sd.Product)
            .WithMany(p => p.SaleDetails)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}