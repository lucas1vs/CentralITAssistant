
namespace CentralITAssistant.Domain.Entities;

public class Employee
{
    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string PersonalEmail { get; private set; } = string.Empty;

    public string? CorporateEmail { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ICollection<WhatsAppContact> WhatsAppContacts { get; private set; }
        = new List<WhatsAppContact>();

    public ICollection<Conversation> Conversations { get; private set; }
        = new List<Conversation>();

    // Construtor utilizado pelo Entity Framework Core
    private Employee()
    {
    }

    // Criação de um novo colaborador
    public Employee(
        string fullName,
        string personalEmail,
        string? corporateEmail = null)
    {
        SetFullName(fullName);
        SetPersonalEmail(personalEmail);
        SetCorporateEmail(corporateEmail);

        Id = Guid.NewGuid();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // Associação de um novo contato WhatsApp
    public void AssociateWhatsAppContact(WhatsAppContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        EnsureEmployeeIsActive();
        EnsureContactIsNotAlreadyAssociated(contact);

        if (WhatsAppContacts.Any(c => c.IsActive))
            throw new InvalidOperationException(
                "O colaborador já possui um contato WhatsApp ativo.");

        contact.AssociateWithEmployee(this);

        WhatsAppContacts.Add(contact);
    }

    // Substituição do contato WhatsApp ativo
    public void ReplaceWhatsAppContact(WhatsAppContact newContact)
    {
        ArgumentNullException.ThrowIfNull(newContact);

        EnsureEmployeeIsActive();
        EnsureContactIsNotAlreadyAssociated(newContact);

        var currentContact = WhatsAppContacts.SingleOrDefault(c => c.IsActive);

        if (currentContact is null)
            throw new InvalidOperationException(
                "Não existe um contato WhatsApp ativo para substituir.");

        // Primeiro valida e associa o novo contato.
        newContact.AssociateWithEmployee(this);

        // Depois desativa o contato anterior.
        currentContact.Deactivate();

        WhatsAppContacts.Add(newContact);
    }

    public void UpdatePersonalEmail(string personalEmail)
    {
        SetPersonalEmail(personalEmail);
    }

    public void UpdateCorporateEmail(string? corporateEmail)
    {
        SetCorporateEmail(corporateEmail);
    }

    // Desativação do colaborador e de seus contatos ativos
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        foreach (var contact in WhatsAppContacts.Where(c => c.IsActive))
        {
            contact.Deactivate();
        }
    }

    // Reativação do colaborador, sem reativar automaticamente seus contatos
    public void Activate()
    {
        IsActive = true;
    }

    private void EnsureEmployeeIsActive()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "Não é possível realizar esta operação com um colaborador inativo.");
    }

    private void EnsureContactIsNotAlreadyAssociated(WhatsAppContact contact)
    {
        if (WhatsAppContacts.Any(c =>
            ReferenceEquals(c, contact) || c.Id == contact.Id))
        {
            throw new InvalidOperationException(
                "Este contato já está associado ao colaborador.");
        }
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "O nome do colaborador é obrigatório.");

        FullName = fullName.Trim();
    }

    private void SetPersonalEmail(string personalEmail)
    {
        if (string.IsNullOrWhiteSpace(personalEmail))
            throw new ArgumentException(
                "O e-mail pessoal é obrigatório.");

        PersonalEmail = personalEmail.Trim().ToLowerInvariant();
    }

    private void SetCorporateEmail(string? corporateEmail)
    {
        CorporateEmail = string.IsNullOrWhiteSpace(corporateEmail)
            ? null
            : corporateEmail.Trim().ToLowerInvariant();
    }
}
