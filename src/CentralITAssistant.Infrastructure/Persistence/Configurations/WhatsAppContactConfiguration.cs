
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralITAssistant.Infrastructure.Persistence.Configurations;

public class WhatsAppContactConfiguration
    : IEntityTypeConfiguration<WhatsAppContact>
{
    public void Configure(EntityTypeBuilder<WhatsAppContact> builder)
    {
        builder.ToTable("WhatsAppContacts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(w => w.PhoneNumber)
            .IsUnique();

        builder.Property(w => w.IsVerified)
            .IsRequired();

        builder.Property(w => w.VerifiedAt);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        // EmployeeId é opcional.
        // O relacionamento 1:1 é configurado em EmployeeConfiguration.
    }
}
