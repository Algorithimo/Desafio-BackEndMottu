using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorcycleRental.Domain.Events;

namespace MotorcycleRental.Infrastructure.Data.Configurations
{
    public class MotorcycleEventConfiguration : IEntityTypeConfiguration<MotorcycleEvent>
    {
        public void Configure(EntityTypeBuilder<MotorcycleEvent> builder)
        {
            builder.ToTable("MotorcycleEvents");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.MotorcycleId)
                .IsRequired();

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(e => e.Year)
                .IsRequired();

            builder.Property(e => e.EventData)
                .IsRequired()
                .HasColumnType("jsonb"); // PostgreSQL JSON

            builder.Property(e => e.ProcessedAt);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Índices
            builder.HasIndex(e => e.Year)
                .HasDatabaseName("IX_MotorcycleEvents_Year");

            builder.HasIndex(e => e.ProcessedAt)
                .HasDatabaseName("IX_MotorcycleEvents_ProcessedAt");

            builder.HasIndex(e => new { e.Year, e.ProcessedAt })
                .HasDatabaseName("IX_MotorcycleEvents_Year_ProcessedAt");
        }
    }
}