
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralITAssistant.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.PersonalEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.PersonalEmail)
            .IsUnique();

        builder.Property(e => e.CorporateEmail)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.HasIndex(e => e.CorporateEmail)
            .IsUnique()
            .HasFilter("[CorporateEmail] IS NOT NULL");

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Employee 1:N WhatsAppContacts
        builder.HasMany(e => e.WhatsAppContacts)
            .WithOne(w => w.Employee)
            .HasForeignKey(w => w.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
