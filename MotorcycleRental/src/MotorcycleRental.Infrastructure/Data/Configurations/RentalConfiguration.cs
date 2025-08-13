using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Infrastructure.Data.Configurations
{
    public class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.ToTable("Rentals");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedNever();

            builder.Property(r => r.MotorcycleId)
                .IsRequired();

            builder.Property(r => r.DriverId)
                .IsRequired();

            builder.Property(r => r.Plan)
                .IsRequired()
                .HasConversion<string>();

            // Configurar Value Object RentalPeriod
            builder.OwnsOne(r => r.Period, period =>
            {
                period.Property(p => p.StartDate)
                    .HasColumnName("StartDate")
                    .IsRequired()
                    .HasColumnType("date");

                period.Property(p => p.EndDate)
                    .HasColumnName("EndDate")
                    .HasColumnType("date");

                period.Property(p => p.ExpectedEndDate)
                    .HasColumnName("ExpectedEndDate")
                    .IsRequired()
                    .HasColumnType("date");
            });

            builder.Property(r => r.DailyRate)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(r => r.TotalAmount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(r => r.ReturnDate)
                .HasColumnType("date");

            builder.Property(r => r.PenaltyAmount)
                .HasPrecision(10, 2);

            builder.Property(r => r.AdditionalAmount)
                .HasPrecision(10, 2);

            builder.Property(r => r.FinalAmount)
                .HasPrecision(10, 2);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(r => r.Motorcycle)
                .WithMany(m => m.Rentals)
                .HasForeignKey(r => r.MotorcycleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Driver)
                .WithMany(d => d.Rentals)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(r => r.Status)
                .HasDatabaseName("IX_Rentals_Status");

            builder.HasIndex(r => new { r.MotorcycleId, r.Status })
                .HasDatabaseName("IX_Rentals_MotorcycleId_Status");

            builder.HasIndex(r => new { r.DriverId, r.Status })
                .HasDatabaseName("IX_Rentals_DriverId_Status");
        }
    }
}