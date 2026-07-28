using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;

public class CreditPaymentConfiguration : IEntityTypeConfiguration<CreditPayment>
{
    public void Configure(EntityTypeBuilder<CreditPayment> builder)
    {
        builder.ToTable("CreditPayments");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        
        // ─── Propiedades
        builder.Property(cp => cp.Date)
            .IsRequired();
        
        builder.Property(cp => cp.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(cp => cp.Notes)
            .HasMaxLength(500);

        // Historial de pagos por cliente — consulta muy frecuente
        builder.HasIndex(cp => cp.CustomerId)
            .HasDatabaseName("IX_CreditPayments_CustomerId");

        builder.HasIndex(cp => cp.Date)
            .HasDatabaseName("IX_CreditPayments_Date");

        // ─── Relaciones 
         builder.HasOne(cp => cp.Customer)
            .WithMany(c => c.CreditPayments)
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.User)
            .WithMany(u => u.CreditPayments)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
