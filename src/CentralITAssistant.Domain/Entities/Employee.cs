
namespace CentralITAssistant.Domain.Entities;

public class Employee
{
    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string CorporateEmail { get; private set; } = string.Empty;

    public string? PersonalEmail { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public WhatsAppContact? WhatsAppContact { get; private set; }

    public ICollection<Conversation> Conversations { get; private set; } = new List<Conversation>();

    // Construtor utilizado pelo Entity Framework Core
    private Employee()
    {
    }

    // Criação de um novo colaborador
    public Employee(
        string fullName,
        string corporateEmail,
        string? personalEmail = null)
    {
        SetFullName(fullName);
        SetCorporateEmail(corporateEmail);
        SetPersonalEmail(personalEmail);

        Id = Guid.NewGuid();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // Adição do contato WhatsApp

    public void AssociateWhatsAppContact(WhatsAppContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (WhatsAppContact is not null)
            throw new InvalidOperationException(
                "O colaborador já possui um contato WhatsApp cadastrado.");

        contact.AssociateWithEmployee(this);

        WhatsAppContact = contact;
    }


    public void UpdatePersonalEmail(string? personalEmail)
    {
        SetPersonalEmail(personalEmail);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "O nome do colaborador é obrigatório.");

        FullName = fullName.Trim();
    }

    private void SetCorporateEmail(string corporateEmail)
    {
        if (string.IsNullOrWhiteSpace(corporateEmail))
            throw new ArgumentException(
                "O e-mail corporativo é obrigatório.");

        CorporateEmail = corporateEmail.Trim().ToLowerInvariant();
    }

    private void SetPersonalEmail(string? personalEmail)
    {
        PersonalEmail = string.IsNullOrWhiteSpace(personalEmail)
            ? null
            : personalEmail.Trim().ToLowerInvariant();
    }
}
