
namespace CentralITAssistant.Domain.Entities;

public class WhatsAppContact
{
    public Guid Id { get; private set; }

    public Guid? EmployeeId { get; private set; }

    public string PhoneNumber { get; private set; } = string.Empty;

    public bool IsVerified { get; private set; }

    public DateTime? VerifiedAt { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Employee? Employee { get; private set; }

    public ICollection<Conversation> Conversations { get; private set; }
        = new List<Conversation>();

    // Construtor utilizado pelo Entity Framework Core
    private WhatsAppContact()
    {
    }

    // Criação de um novo contato, ainda não identificado
    public WhatsAppContact(string phoneNumber)
    {
        SetPhoneNumber(phoneNumber);

        Id = Guid.NewGuid();
        IsVerified = false;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // Associação após a identificação do colaborador
    internal void AssociateWithEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (!employee.IsActive)
            throw new InvalidOperationException(
                "Não é possível associar um contato a um colaborador inativo.");

        if (!IsActive)
            throw new InvalidOperationException(
                "Não é possível associar um contato WhatsApp inativo.");

        if (EmployeeId.HasValue && EmployeeId != employee.Id)
            throw new InvalidOperationException(
                "Este contato já está associado a outro colaborador.");

        if (Employee is not null && !ReferenceEquals(Employee, employee))
            throw new InvalidOperationException(
                "Este contato já está associado a outro colaborador.");

        EmployeeId = employee.Id;
        Employee = employee;
    }

    // Atualização do número
    public void UpdatePhoneNumber(string phoneNumber)
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "Não é possível alterar o número de um contato inativo.");

        string normalizedPhone = NormalizePhoneNumber(phoneNumber);

        if (PhoneNumber == normalizedPhone)
            return;

        PhoneNumber = normalizedPhone;

        IsVerified = false;
        VerifiedAt = null;
    }

    // Verificação após confirmação da identidade
    public void Verify()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "Não é possível verificar um contato inativo.");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        if (Employee is not null)
        {
            if (!Employee.IsActive)
                throw new InvalidOperationException(
                    "Não é possível ativar um contato de um colaborador inativo.");

            if (Employee.WhatsAppContacts.Any(c =>
                c.IsActive && !ReferenceEquals(c, this)))
            {
                throw new InvalidOperationException(
                    "O colaborador já possui outro contato WhatsApp ativo.");
            }
        }

        IsActive = true;

        // A reativação exige uma nova verificação do número.
        IsVerified = false;
        VerifiedAt = null;
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException(
                "O número de WhatsApp é obrigatório.");

        return phoneNumber.Trim();
    }
}
