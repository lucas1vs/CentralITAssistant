
namespace CentralITAssistant.Domain.Entities;

public class Conversation
{
    public Guid Id { get; private set; }

    public Guid? EmployeeId { get; private set; }

    public Guid WhatsAppContactId { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public bool IsActive { get; private set; }

    public Employee? Employee { get; private set; }

    public WhatsAppContact WhatsAppContact { get; private set; } = null!;

    // Construtor utilizado pelo Entity Framework Core
    private Conversation()
    {
    }

    // Criação de uma conversa a partir do contato WhatsApp
    public Conversation(WhatsAppContact whatsAppContact)
    {
        ArgumentNullException.ThrowIfNull(whatsAppContact);

        Id = Guid.NewGuid();
        WhatsAppContactId = whatsAppContact.Id;
        WhatsAppContact = whatsAppContact;
        StartedAt = DateTime.UtcNow;
        IsActive = true;
    }

    // Associação do colaborador após a identificação
    public void AssociateEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (WhatsAppContact.EmployeeId != employee.Id)
            throw new InvalidOperationException(
                "O contato WhatsApp não pertence ao colaborador informado.");

        if (EmployeeId.HasValue && EmployeeId != employee.Id)
            throw new InvalidOperationException(
                "A conversa já está associada a outro colaborador.");

        EmployeeId = employee.Id;
        Employee = employee;
    }

    // Encerramento da conversa
    public void Close()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "A conversa já está encerrada.");

        IsActive = false;
        EndedAt = DateTime.UtcNow;
    }
}
