using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // ─── Propiedades ──────────────────────────────────────────────────────

        builder.Property(s => s.TicketNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.Date)
            .IsRequired();

        builder.Property(s => s.PaymentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.IGV)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.Total)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);


        // ─── Índices ──────────────────────────────────────────────────────────

        builder.HasIndex(s => s.TicketNumber)
            .IsUnique()
            .HasDatabaseName("IX_Sales_TicketNumber");

        // Índice en Date — las consultas por rango de fecha son muy frecuentes
        builder.HasIndex(s => s.Date)
            .HasDatabaseName("IX_Sales_Date");

        // ─── Relaciones ───────────────────────────────────────────────────────

        // Venta → Usuario (obligatorio)
        builder.HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Venta → Cliente (opcional — no toda venta tiene cliente)
        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Venta → CashRegister (opcional)
        builder.HasOne(s => s.CashRegister)
            .WithMany(cr => cr.Sales)
            .HasForeignKey(s => s.CashRegisterId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ─── Colección privada de detalles ────────────────────────────────────

        builder.HasMany(s => s.Details)
            .WithOne(d => d.Sale)
            .HasForeignKey(d => d.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}