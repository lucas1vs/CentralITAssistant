
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralITAssistant.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration
    : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.StartedAt)
            .IsRequired();

        builder.Property(c => c.EndedAt);

        builder.Property(c => c.IsActive)
            .IsRequired();

        // Employee 1:N Conversation
        // EmployeeId pode ser nulo antes da identificação.
        builder.HasOne(c => c.Employee)
            .WithMany(e => e.Conversations)
            .HasForeignKey(c => c.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // WhatsAppContact 1:N Conversation
        // Toda conversa precisa ter um contato.
        builder.HasOne(c => c.WhatsAppContact)
            .WithMany(w => w.Conversations)
            .HasForeignKey(c => c.WhatsAppContactId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Apenas uma conversa ativa por colaborador.
        // Conversas ainda não identificadas podem coexistir.
        builder.HasIndex(c => c.EmployeeId)
            .IsUnique()
            .HasFilter(
                "[EmployeeId] IS NOT NULL AND [IsActive] = 1");
    }
}
