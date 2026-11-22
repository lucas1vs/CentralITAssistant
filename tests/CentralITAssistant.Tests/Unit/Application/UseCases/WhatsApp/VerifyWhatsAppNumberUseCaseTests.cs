
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.WhatsApp;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Application.UseCases.WhatsApp;

public class VerifyWhatsAppNumberUseCaseTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private const string ValidCode = "123456";

    private static PendingWhatsAppVerification CreateVerification(
        Guid employeeId,
        string phoneNumber = "+5561999999999",
        DateTime? sentAt = null,
        DateTime? expiresAt = null)
    {
        var sent = sentAt ?? Now;

        return new PendingWhatsAppVerification(
            employeeId,
            phoneNumber,
            $"hash:{ValidCode}",
            expiresAt ?? sent.AddMinutes(5),
            sent);
    }

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

    private static VerifyWhatsAppNumberUseCase CreateUseCase(
        FakeEmployeeRepository employeeRepository,
        FakePendingWhatsAppVerificationRepository verificationRepository,
        FakeVerificationCodeService? codeService = null)
    {
        return new VerifyWhatsAppNumberUseCase(
            employeeRepository,
            verificationRepository,
            codeService ?? new FakeVerificationCodeService());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFirstNumberIsVerified_ShouldVerifyExistingContact()
    {
        var employee = CreateEmployee("+5561999999999");
        var verification = CreateVerification(employee.Id);

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var result = await useCase.ExecuteAsync(
            verification.Id,
            ValidCode);

        var contact = Assert.Single(employee.WhatsAppContacts);

        Assert.Equal(employee.Id, result);
        Assert.True(contact.IsVerified);
        Assert.NotNull(contact.VerifiedAt);
        Assert.True(verification.IsVerified);
        Assert.Equal(1, verificationRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenChangingNumber_ShouldReplaceOldContact()
    {
        var employee = CreateEmployee(
            "+5561988888888",
            verified: true);

        var oldContact = Assert.Single(employee.WhatsAppContacts);
        var newPhoneNumber = "+5561999999999";

        var verification = CreateVerification(
            employee.Id,
            newPhoneNumber);

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var result = await useCase.ExecuteAsync(
            verification.Id,
            ValidCode);

        Assert.Equal(employee.Id, result);
        Assert.False(oldContact.IsActive);

        Assert.Equal(2, employee.WhatsAppContacts.Count);

        var newContact = employee.WhatsAppContacts
            .Single(contact => contact.IsActive);

        Assert.Equal(newPhoneNumber, newContact.PhoneNumber);
        Assert.True(newContact.IsVerified);
        Assert.NotNull(newContact.VerifiedAt);

        Assert.True(verification.IsVerified);
        Assert.Equal(1, verificationRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeHasNoContact_ShouldCreateAndAssociateContact()
    {
        var employee = new Employee(
            "Funcionário Teste",
            "funcionario@email.com");

        var verification = CreateVerification(employee.Id);

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        await useCase.ExecuteAsync(verification.Id, ValidCode);

        var contact = Assert.Single(employee.WhatsAppContacts);

        Assert.Equal(verification.PhoneNumber, contact.PhoneNumber);
        Assert.True(contact.IsVerified);
        Assert.True(contact.IsActive);
        Assert.True(verification.IsVerified);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeIsInvalid_ShouldRegisterFailedAttempt()
    {
        var employee = CreateEmployee();
        var verification = CreateVerification(employee.Id);

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                "000000"));

        Assert.Equal("Código de verificação inválido.", exception.Message);
        Assert.Equal(1, verification.FailedAttempts);
        Assert.False(verification.IsVerified);
        Assert.Equal(1, verificationRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationDoesNotExist_ShouldThrowException()
    {
        var employeeRepository = new FakeEmployeeRepository();
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository();

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), ValidCode));

        Assert.Equal("Verificação não encontrada.", exception.Message);
        Assert.Equal(0, verificationRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeDoesNotExist_ShouldThrowException()
    {
        var employeeId = Guid.NewGuid();
        var verification = CreateVerification(employeeId);

        var employeeRepository = new FakeEmployeeRepository();
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                ValidCode));

        Assert.Equal("Funcionário não encontrado.", exception.Message);
        Assert.False(verification.IsVerified);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeIsInactive_ShouldThrowException()
    {
        var employee = CreateEmployee();
        employee.Deactivate();

        var verification = CreateVerification(employee.Id);

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                ValidCode));

        Assert.Equal(
            "Funcionário inativo não pode verificar um número.",
            exception.Message);

        Assert.False(verification.IsVerified);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIsExpired_ShouldThrowException()
    {
        var employee = CreateEmployee();

        var verification = CreateVerification(
            employee.Id,
            sentAt: Now.AddMinutes(-10),
            expiresAt: Now.AddMinutes(-5));

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                ValidCode));

        Assert.Contains("expirou", exception.Message);
        Assert.False(verification.IsVerified);
        Assert.Equal(0, verificationRepository.SaveChangesCount);
    }

    [Fact]

   
    public async Task ExecuteAsync_WhenMaximumAttemptsReached_ShouldThrowException()
    {
        var employee = CreateEmployee();
        var oldContact = Assert.Single(employee.WhatsAppContacts);

        var verification = CreateVerification(employee.Id);

        for (var i = 0; i < PendingWhatsAppVerification.MaxFailedAttempts; i++)
        {
            verification.RegisterFailedAttempt(Now.AddMinutes(1));
        }

        var employeeRepository = new FakeEmployeeRepository(employee);

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                ValidCode));

        Assert.Contains("limite de tentativas", exception.Message);

        Assert.Equal(
            PendingWhatsAppVerification.MaxFailedAttempts,
            verification.FailedAttempts);

        Assert.False(verification.IsVerified);

        Assert.Equal(0, verificationRepository.SaveChangesCount);

        Assert.True(oldContact.IsActive);
    }
    


    [Fact]
    public async Task ExecuteAsync_WhenVerificationIsCancelled_ShouldThrowException()
    {
        var employee = CreateEmployee();
        var verification = CreateVerification(employee.Id);

        verification.Cancel();

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                ValidCode));

        Assert.Contains("cancelada", exception.Message);
        Assert.False(verification.IsVerified);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIsAlreadyCompleted_ShouldThrowException()
    {
        var employee = CreateEmployee();
        var verification = CreateVerification(employee.Id);

        verification.MarkAsVerified(Now.AddMinutes(1));

        var employeeRepository = new FakeEmployeeRepository(employee);
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                ValidCode));

        Assert.Contains("expirou, foi cancelada ou atingiu", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIdIsEmpty_ShouldThrowException()
    {
        var useCase = CreateUseCase(
            new FakeEmployeeRepository(),
            new FakePendingWhatsAppVerificationRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(Guid.Empty, ValidCode));

        Assert.Equal("verificationId", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteAsync_WhenCodeIsEmpty_ShouldThrowException(
        string? code)
    {
        var useCase = CreateUseCase(
            new FakeEmployeeRepository(),
            new FakePendingWhatsAppVerificationRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), code!));

        Assert.Equal("code", exception.ParamName);
    }


    [Fact]
    public async Task ExecuteAsync_WhenCodeIsInvalid_ShouldNotChangeExistingContact()
    {
        var employee = CreateEmployee(
            "+5561988888888",
            verified: true);

        var oldContact = Assert.Single(employee.WhatsAppContacts);

        var verification = CreateVerification(
            employee.Id,
            "+5561999999999");

        var employeeRepository = new FakeEmployeeRepository(employee);

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var useCase = CreateUseCase(
            employeeRepository,
            verificationRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                verification.Id,
                "000000"));

        Assert.True(oldContact.IsActive);
        Assert.True(oldContact.IsVerified);

        Assert.Single(employee.WhatsAppContacts);

        Assert.False(verification.IsVerified);
        Assert.Equal(1, verification.FailedAttempts);
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

        public Task<Employee?> GetByIdWithWhatsAppContactsAsync(
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

        public Task<Employee?> GetByCorporateEmailAsync(
            string corporateEmail,
            CancellationToken cancellationToken = default)
        {
            var employee = _employees.Values.FirstOrDefault(
                e => e.CorporateEmail == corporateEmail);

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

    private sealed class FakeVerificationCodeService
        : IVerificationCodeService
    {
        public string GenerateCode() => ValidCode;

        public string HashCode(string code) => $"hash:{code}";

        public bool VerifyCode(string code, string hash)
            => hash == $"hash:{code}";
    }
}
