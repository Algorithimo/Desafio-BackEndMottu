using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Infrastructure.Data.Configurations
{
    public class MotorcycleConfiguration : IEntityTypeConfiguration<Motorcycle>
    {
        public void Configure(EntityTypeBuilder<Motorcycle> builder)
        {
            builder.ToTable("Motorcycles");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .ValueGeneratedNever(); // Já geramos o Guid no Domain

            builder.Property(m => m.Identifier)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Year)
                .IsRequired();

            builder.Property(m => m.Model)
                .IsRequired()
                .HasMaxLength(100);

            // Configurar Value Object Plate
            builder.OwnsOne(m => m.Plate, plate =>
            {
                plate.Property(p => p.Value)
                    .HasColumnName("Plate")
                    .IsRequired()
                    .HasMaxLength(10);

                // Índice único para placa
                plate.HasIndex(p => p.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Motorcycles_Plate");
            });

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .IsRequired();

            // Relacionamento com Rentals
            builder.HasMany(m => m.Rentals)
                .WithOne(r => r.Motorcycle)
                .HasForeignKey(r => r.MotorcycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(m => m.Identifier)
                .HasDatabaseName("IX_Motorcycles_Identifier");
        }
    }
}