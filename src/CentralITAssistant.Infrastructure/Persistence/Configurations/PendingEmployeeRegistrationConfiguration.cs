
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralITAssistant.Infrastructure.Persistence.Configurations;

public class PendingEmployeeRegistrationConfiguration
    : IEntityTypeConfiguration<PendingEmployeeRegistration>
{
    public void Configure(
        EntityTypeBuilder<PendingEmployeeRegistration> builder)
    {
        builder.ToTable("PendingEmployeeRegistrations");

        builder.HasKey(registration => registration.Id);

        builder.Property(registration => registration.Id)
            .ValueGeneratedNever();

        builder.Property(registration => registration.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(registration => registration.PersonalEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(registration => registration.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(registration => registration.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(registration => registration.CreatedAt)
            .IsRequired();

        builder.Property(registration => registration.ExpiresAt)
            .IsRequired();

        builder.Property(registration => registration.LastCodeSentAt)
            .IsRequired();

        builder.Property(registration => registration.ResendCount)
            .IsRequired();

        builder.Property(registration => registration.FailedAttempts)
            .IsRequired();

        builder.Property(registration => registration.IsVerified)
            .IsRequired();

        builder.HasIndex(registration => registration.PersonalEmail)
            .IsUnique();
    }
}
