
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.WhatsApp;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Application.UseCases.WhatsApp;

public class RequestWhatsAppVerificationUseCaseTests
{
    private const string ValidCode = "123456";

    private static Employee CreateEmployee(
        string phoneNumber = "+5561988888888",
        bool verified = false)
    {
        var employee = new Employee(
            "Funcionário Teste",
            "funcionario@email.com");

        var contact = new WhatsAppContact(phoneNumber);

        if (verified)
            contact.Verify();

        employee.AssociateWhatsAppContact(contact);

        return employee;
    }

    private static PendingWhatsAppVerification CreateVerification(
        Guid employeeId,
        string phoneNumber = "+5561999999999")
    {
        var now = DateTime.UtcNow;

        return new PendingWhatsAppVerification(
            employeeId,
            phoneNumber,
            $"hash:{ValidCode}",
            now.AddMinutes(5),
            now);
    }

    private static RequestWhatsAppVerificationUseCase CreateUseCase(
        FakeEmployeeRepository? employeeRepository = null,
        FakePendingWhatsAppVerificationRepository? verificationRepository = null,
        FakeWhatsAppContactRepository? contactRepository = null,
        FakeVerificationCodeService? codeService = null,
        FakeWhatsAppService? whatsAppService = null)
    {
        return new RequestWhatsAppVerificationUseCase(
            employeeRepository ?? new FakeEmployeeRepository(),
            verificationRepository ?? new FakePendingWhatsAppVerificationRepository(),
            contactRepository ?? new FakeWhatsAppContactRepository(),
            codeService ?? new FakeVerificationCodeService(),
            whatsAppService ?? new FakeWhatsAppService());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDataIsValid_ShouldCreateVerificationAndSendCode()
    {
        // Arrange
        var employee = CreateEmployee();
        var phoneNumber = "+5561999999999";

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository();

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository,
            whatsAppService: whatsAppService);

        // Act
        var result = await useCase.ExecuteAsync(
            employee.Id,
            phoneNumber);

        // Assert
        var verification = Assert.Single(
            verificationRepository.Verifications);

        Assert.NotEqual(Guid.Empty, result);
        Assert.Equal(result, verification.Id);
        Assert.Equal(employee.Id, verification.EmployeeId);
        Assert.Equal(phoneNumber, verification.PhoneNumber);
        Assert.False(verification.IsVerified);

        Assert.Equal(1, verificationRepository.SaveChangesCount);

        Assert.Equal(phoneNumber, whatsAppService.LastPhoneNumber);
        Assert.Equal(ValidCode, whatsAppService.LastCode);
        Assert.Equal(1, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(
                Guid.NewGuid(),
                "+5561999999999"));

        // Assert
        Assert.Equal("Funcionário não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeIsInactive_ShouldThrowException()
    {
        // Arrange
        var employee = CreateEmployee();
        employee.Deactivate();

        var useCase = CreateUseCase(
            new FakeEmployeeRepository(employee));

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                employee.Id,
                "+5561999999999"));

        // Assert
        Assert.Equal(
            "Funcionário inativo não pode solicitar verificação.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNumberIsAlreadyVerifiedAndActive_ShouldThrowException()
    {
        // Arrange
        var phoneNumber = "+5561988888888";
        var employee = CreateEmployee(phoneNumber, verified: true);

        var useCase = CreateUseCase(
            new FakeEmployeeRepository(employee));

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(employee.Id, phoneNumber));

        // Assert
        Assert.Equal(
            "O número informado já está verificado e ativo.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNumberBelongsToAnotherEmployee_ShouldThrowException()
    {
        // Arrange
        var employee = CreateEmployee();

        var anotherEmployee = CreateEmployee(
            "+5561999999999");

        var existingContact = Assert.Single(
            anotherEmployee.WhatsAppContacts);

        var contactRepository = new FakeWhatsAppContactRepository(
            existingContact);

        var useCase = CreateUseCase(
            new FakeEmployeeRepository(employee, anotherEmployee),
            contactRepository: contactRepository);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                employee.Id,
                "+5561999999999"));

        // Assert
        Assert.Equal(
            "Não foi possível utilizar o número informado.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNumberBelongsToInactiveContact_ShouldThrowException()
    {
        // Arrange
        var employee = CreateEmployee();

        var anotherEmployee = CreateEmployee(
            "+5561999999999");

        var oldContact = Assert.Single(
            anotherEmployee.WhatsAppContacts);

        oldContact.Deactivate();

        var contactRepository = new FakeWhatsAppContactRepository(
            oldContact);

        var useCase = CreateUseCase(
            new FakeEmployeeRepository(employee, anotherEmployee),
            contactRepository: contactRepository);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                employee.Id,
                "+5561999999999"));

        // Assert
        Assert.Equal(
            "Não foi possível utilizar o número informado.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNumberBelongsToSameEmployeeAndIsNotVerified_ShouldAllowRequest()
    {
        // Arrange
        var phoneNumber = "+5561988888888";
        var employee = CreateEmployee(phoneNumber);

        var ownContact = Assert.Single(employee.WhatsAppContacts);

        var contactRepository = new FakeWhatsAppContactRepository(
            ownContact);

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository();

        var useCase = CreateUseCase(
            new FakeEmployeeRepository(employee),
            verificationRepository,
            contactRepository);

        // Act
        var result = await useCase.ExecuteAsync(
            employee.Id,
            phoneNumber);

        // Assert
        var verification = Assert.Single(
            verificationRepository.Verifications);

        Assert.Equal(verification.Id, result);
        Assert.Equal(phoneNumber, verification.PhoneNumber);
        Assert.False(ownContact.IsVerified);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPreviousVerificationExists_ShouldCancelPreviousAndCreateNew()
    {
        // Arrange
        var employee = CreateEmployee();

        var previousVerification = CreateVerification(
            employee.Id,
            "+5561977777777");

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(
                previousVerification);

        var useCase = CreateUseCase(
            new FakeEmployeeRepository(employee),
            verificationRepository);

        // Act
        var result = await useCase.ExecuteAsync(
            employee.Id,
            "+5561999999999");

        // Assert
        Assert.True(previousVerification.IsCancelled);

        Assert.Equal(2, verificationRepository.Verifications.Count);

        var newVerification = verificationRepository.Verifications
            .Single(verification => verification.Id == result);

        Assert.Equal("+5561999999999", newVerification.PhoneNumber);
        Assert.False(newVerification.IsCancelled);

        Assert.Equal(1, verificationRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeIdIsEmpty_ShouldThrowException()
    {
        // Arrange
        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                Guid.Empty,
                "+5561999999999"));

        // Assert
        Assert.Equal("employeeId", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteAsync_WhenPhoneNumberIsEmpty_ShouldThrowException(
        string? phoneNumber)
    {
        // Arrange
        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                Guid.NewGuid(),
                phoneNumber!));

        // Assert
        Assert.Equal("phoneNumber", exception.ParamName);
    }

    // Fakes

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly Dictionary<Guid, Employee> _employees = new();

        public FakeEmployeeRepository(params Employee[] employees)
        {
            foreach (var employee in employees)
                _employees[employee.Id] = employee;
        }

        public Task<Employee?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _employees.TryGetValue(id, out var employee);
            return Task.FromResult(employee);
        }

        public Task AddAsync(
            Employee employee,
            CancellationToken cancellationToken = default)
        {
            _employees[employee.Id] = employee;
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByPersonalEmailAsync(
            string personalEmail,
            CancellationToken cancellationToken = default)
        {
            var employee = _employees.Values.FirstOrDefault(
                e => e.PersonalEmail == personalEmail);

            return Task.FromResult(employee);
        }

        public Task<IReadOnlyList<Employee>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Employee>>(
                _employees.Values.ToList());
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePendingWhatsAppVerificationRepository
        : IPendingWhatsAppVerificationRepository
    {
        private readonly Dictionary<Guid, PendingWhatsAppVerification>
            _verifications = new();

        public int SaveChangesCount { get; private set; }

        public IReadOnlyCollection<PendingWhatsAppVerification> Verifications
            => _verifications.Values.ToList();

        public FakePendingWhatsAppVerificationRepository(
            params PendingWhatsAppVerification[] verifications)
        {
            foreach (var verification in verifications)
                _verifications[verification.Id] = verification;
        }

        public Task<PendingWhatsAppVerification?> GetByIdAsync(Guid id)
        {
            _verifications.TryGetValue(id, out var verification);
            return Task.FromResult(verification);
        }

        public Task<PendingWhatsAppVerification?>
            GetActiveByEmployeeIdAsync(Guid employeeId)
        {
            var verification = _verifications.Values.FirstOrDefault(
                v => v.EmployeeId == employeeId
                     && !v.IsVerified
                     && !v.IsCancelled);

            return Task.FromResult(verification);
        }

        public Task AddAsync(PendingWhatsAppVerification verification)
        {
            _verifications[verification.Id] = verification;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeWhatsAppContactRepository
        : IWhatsAppContactRepository
    {
        private readonly List<WhatsAppContact> _contacts;

        public FakeWhatsAppContactRepository(
            params WhatsAppContact[] contacts)
        {
            _contacts = contacts.ToList();
        }

        public Task<bool> ExistsByPhoneNumberForAnotherEmployeeAsync(
            string phoneNumber,
            Guid employeeId)
        {
            var exists = _contacts.Any(contact =>
                contact.PhoneNumber == phoneNumber &&
                contact.EmployeeId != employeeId);

            return Task.FromResult(exists);
        }
    }

    private sealed class FakeVerificationCodeService
        : IVerificationCodeService
    {
        public string GenerateCode() => ValidCode;

        public string HashCode(string code) => $"hash:{code}";

        public bool VerifyCode(string code, string hash)
            => hash == $"hash:{code}";
    }

    private sealed class FakeWhatsAppService : IWhatsAppService
    {
        public int SendCount { get; private set; }

        public string? LastPhoneNumber { get; private set; }

        public string? LastCode { get; private set; }

        public Task SendVerificationCodeAsync(
            string phoneNumber,
            string code)
        {
            SendCount++;
            LastPhoneNumber = phoneNumber;
            LastCode = code;

            return Task.CompletedTask;
        }
    }
}
