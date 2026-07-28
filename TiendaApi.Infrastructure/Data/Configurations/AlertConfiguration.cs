using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        
        // ─── Propiedades ──────────────────────────────────────────────────────
        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.IsRead)
            .HasDefaultValue(false);

        // Consulta más frecuente: alertas no leídas para el dashboard
        builder.HasIndex(a => a.IsRead)
            .HasDatabaseName("IX_Alerts_IsRead");

        builder.HasIndex(a => a.Type)
            .HasDatabaseName("IX_Alerts_Type");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_Alerts_CreatedAt");

    }
}