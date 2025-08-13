using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Infrastructure.Data.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Drivers");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedNever();

            builder.Property(d => d.Identifier)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Configurar Value Object CNPJ
            builder.OwnsOne(d => d.CNPJ, cnpj =>
            {
                cnpj.Property(c => c.Value)
                    .HasColumnName("CNPJ")
                    .IsRequired()
                    .HasMaxLength(14);

                cnpj.HasIndex(c => c.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Drivers_CNPJ");
            });

            builder.Property(d => d.BirthDate)
                .IsRequired()
                .HasColumnType("date");

            // Configurar Value Object CNH
            builder.OwnsOne(d => d.CNH, cnh =>
            {
                cnh.Property(c => c.Number)
                    .HasColumnName("CNHNumber")
                    .IsRequired()
                    .HasMaxLength(11);

                cnh.HasIndex(c => c.Number)
                    .IsUnique()
                    .HasDatabaseName("IX_Drivers_CNH");
            });

            builder.Property(d => d.CNHType)
                .IsRequired()
                .HasConversion<string>() // Salva como string no banco
                .HasMaxLength(2);

            builder.Property(d => d.CNHImageUrl)
                .HasMaxLength(500);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt)
                .IsRequired();

            // Relacionamento com Rentals
            builder.HasMany(d => d.Rentals)
                .WithOne(r => r.Driver)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(d => d.Identifier)
                .IsUnique()
                .HasDatabaseName("IX_Drivers_Identifier");
        }
    }
}