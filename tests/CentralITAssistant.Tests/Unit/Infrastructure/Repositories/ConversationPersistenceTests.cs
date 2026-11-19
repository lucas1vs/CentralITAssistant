
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Tests.Unit.Infrastructure.Repositories;

public class ConversationPersistenceTests
{
    [Fact]
    public async Task ReplaceWhatsAppContact_ShouldPreserveConversationHistory()
    {
        // Arrange: cria o banco SQLite em memória.
        await using var database = await SqliteTestDatabase.CreateAsync();

        var context = database.Context;

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@email.com");

        var oldContact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(oldContact);

        var conversation = new Conversation(oldContact);

        conversation.AssociateEmployee(employee);

        context.Set<Employee>().Add(employee);
        context.Set<Conversation>().Add(conversation);

        await context.SaveChangesAsync();

        // Guarda os identificadores para consultar após a persistência.
        var employeeId = employee.Id;
        var oldContactId = oldContact.Id;
        var conversationId = conversation.Id;

        // Simula uma nova consulta ao banco.
        context.ChangeTracker.Clear();

        // Act: recupera o funcionário e seus contatos.
        var persistedEmployee = await context.Set<Employee>()
            .Include(e => e.WhatsAppContacts)
            .SingleAsync(e => e.Id == employeeId);

        var newContact = new WhatsAppContact("5561988888888");

        persistedEmployee.ReplaceWhatsAppContact(newContact);

        var newContactId = newContact.Id;

        // Tenta persistir a substituição e identifica possíveis conflitos.
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var diagnostic = new System.Text.StringBuilder();

            diagnostic.AppendLine("=== DIAGNÓSTICO DE CONCORRÊNCIA ===");

            foreach (var entry in ex.Entries)
            {
                diagnostic.AppendLine(
                    $"Entidade: {entry.Metadata.ClrType.Name}");

                diagnostic.AppendLine(
                    $"Estado: {entry.State}");

                foreach (var property in entry.Properties)
                {
                    diagnostic.AppendLine(
                        $"{property.Metadata.Name}: {property.CurrentValue}");
                }

                diagnostic.AppendLine("-----------------------------------");
            }

            throw new InvalidOperationException(
                diagnostic.ToString(),
                ex);
        }

        // Limpa o rastreamento para garantir uma nova leitura do banco.
        context.ChangeTracker.Clear();

        // Recupera a conversa e seus relacionamentos.
        var persistedConversation = await context.Set<Conversation>()
            .Include(c => c.WhatsAppContact)
            .Include(c => c.Employee)
            .SingleAsync(c => c.Id == conversationId);

        var refreshedEmployee = await context.Set<Employee>()
            .Include(e => e.WhatsAppContacts)
            .SingleAsync(e => e.Id == employeeId);

        var persistedOldContact = refreshedEmployee.WhatsAppContacts
            .Single(c => c.Id == oldContactId);

        var persistedNewContact = refreshedEmployee.WhatsAppContacts
            .Single(c => c.Id == newContactId);

        // Assert: a conversa continua vinculada ao contato antigo.
        Assert.Equal(oldContactId, persistedConversation.WhatsAppContactId);
        Assert.Equal(oldContactId, persistedConversation.WhatsAppContact.Id);

        // O funcionário continua sendo o mesmo.
        Assert.Equal(employeeId, persistedConversation.EmployeeId);
        Assert.Equal(employeeId, persistedConversation.Employee!.Id);

        // O contato antigo foi preservado, mas está inativo.
        Assert.False(persistedOldContact.IsActive);

        // O novo contato está ativo.
        Assert.True(persistedNewContact.IsActive);

        // O histórico da conversa não foi alterado.
        Assert.True(persistedConversation.IsActive);
        Assert.Null(persistedConversation.EndedAt);
    }
}
