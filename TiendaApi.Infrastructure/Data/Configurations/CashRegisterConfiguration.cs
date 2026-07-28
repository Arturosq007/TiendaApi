using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;
public class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("CashRegisters");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // ─── Propiedades ──────────────────────────────────────────────────────

        builder.Property(cr => cr.OpenedAt)
            .IsRequired();

        builder.Property(cr => cr.OpeningBalance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(cr => cr.ClosingBalance)
            .HasPrecision(18, 2);

        builder.Property(cr => cr.ExpectedBalance)
            .HasPrecision(18, 2);

        builder.Property(cr => cr.Difference)
            .HasPrecision(18, 2);

        builder.Property(cr => cr.IsOpen)
            .HasDefaultValue(true);

        builder.Property(cr => cr.Notes)
            .HasMaxLength(500);

        // ─── Índices ──────────────────────────────────────────────────────────

        // Consulta frecuente: ¿tiene este cajero un turno abierto?
        builder.HasIndex(cr => new { cr.UserId, cr.IsOpen })
            .HasDatabaseName("IX_CashRegisters_UserId_IsOpen");

        builder.HasIndex(cr => cr.OpenedAt)
            .HasDatabaseName("IX_CashRegisters_OpenedAt");

        builder.HasOne(cr => cr.User)
            .WithMany()          // User no tiene colección de CashRegisters
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
