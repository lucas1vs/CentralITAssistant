
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

        // Um número não pode pertencer a dois registros.
        builder.HasIndex(w => w.PhoneNumber)
            .IsUnique();

        builder.Property(w => w.IsVerified)
            .IsRequired();

        builder.Property(w => w.VerifiedAt);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        // Cada contato pertence a, no máximo, um funcionário.
        builder.HasOne(w => w.Employee)
            .WithMany(e => e.WhatsAppContacts)
            .HasForeignKey(w => w.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Um funcionário pode ter apenas um contato ativo.
        builder.HasIndex(w => w.EmployeeId)
            .IsUnique()
            .HasFilter("[EmployeeId] IS NOT NULL AND [IsActive] = 1");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();
    }
}
