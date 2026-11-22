
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralITAssistant.Infrastructure.Persistence.Configurations;

public class PendingWhatsAppVerificationConfiguration
    : IEntityTypeConfiguration<PendingWhatsAppVerification>
{
    public void Configure(
        EntityTypeBuilder<PendingWhatsAppVerification> builder)
    {
        builder.ToTable("PendingWhatsAppVerifications");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.EmployeeId)
            .IsRequired();

        builder.Property(v => v.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.ExpiresAt)
            .IsRequired();

        builder.Property(v => v.LastCodeSentAt)
            .IsRequired();

        builder.Property(v => v.ResendCount)
            .IsRequired();

        builder.Property(v => v.FailedAttempts)
            .IsRequired();

        builder.Property(v => v.IsVerified)
            .IsRequired();

        builder.Property(v => v.IsCancelled)
            .IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(v => v.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.EmployeeId);

        builder.HasIndex(v => v.PhoneNumber);
    }
}
