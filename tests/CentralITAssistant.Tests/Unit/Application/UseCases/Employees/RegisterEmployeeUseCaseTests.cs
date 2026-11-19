
using System.Reflection;
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.Employees;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Application.UseCases.Employees;

public class RegisterEmployeeUseCaseTests
{
    private readonly FakeEmployeeRepository _employeeRepository = new();
    private readonly FakePendingEmployeeRegistrationRepository _pendingRepository = new();
    private readonly FakeVerificationCodeService _verificationCodeService = new();
    private readonly FakeEmailService _emailService = new();

    private RegisterEmployeeUseCase CreateUseCase()
    {
        return new RegisterEmployeeUseCase(
            _employeeRepository,
            _pendingRepository,
            _verificationCodeService,
            _emailService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRegistrationIsNew_ShouldCreatePendingRegistrationAndSendEmail()
    {
        // Arrange
        var useCase = CreateUseCase();

        // Act
        var registrationId = await useCase.ExecuteAsync(
            "Lucas Domingos",
            "  LUCAS@EMAIL.COM  ",
            "61999999999");

        // Assert
        var registration = _pendingRepository.Registration;

        Assert.NotNull(registration);
        Assert.Equal(registrationId, registration.Id);
        Assert.Equal("Lucas Domingos", registration.FullName);
        Assert.Equal("lucas@email.com", registration.PersonalEmail);
        Assert.Equal("61999999999", registration.PhoneNumber);
        Assert.Equal("hash:123456", registration.CodeHash);
        Assert.False(registration.IsVerified);
        Assert.Equal(0, registration.ResendCount);
        Assert.Equal(0, registration.FailedAttempts);

        Assert.Equal(1, _pendingRepository.AddCount);
        Assert.Equal(1, _pendingRepository.SaveChangesCount);

        Assert.Equal(1, _emailService.SendCount);
        Assert.Equal("lucas@email.com", _emailService.LastEmail);
        Assert.Equal("123456", _emailService.LastVerificationCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmployeeAlreadyExists_ShouldThrowException()
    {
        // Arrange
        _employeeRepository.AddEmployee(
            new Employee("Lucas Domingos", "lucas@email.com"));

        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                "Lucas Domingos",
                "lucas@email.com",
                "61999999999"));

        // Assert
        Assert.Equal(
            "Já existe um funcionário cadastrado com este e-mail.",
            exception.Message);

        Assert.Equal(0, _pendingRepository.AddCount);
        Assert.Equal(0, _pendingRepository.SaveChangesCount);
        Assert.Equal(0, _emailService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPendingRegistrationExists_ShouldRenewVerification()
    {
        // Arrange
        var existingRegistration = CreateExistingRegistration();

        SetPrivateProperty(
            existingRegistration,
            nameof(PendingEmployeeRegistration.LastCodeSentAt),
            DateTime.UtcNow.AddSeconds(-61));

        existingRegistration.RegisterFailedAttempt();
        existingRegistration.RegisterFailedAttempt();

        var originalId = existingRegistration.Id;
        var originalCreatedAt = existingRegistration.CreatedAt;

        _pendingRepository.SetRegistration(existingRegistration);

        var useCase = CreateUseCase();

        // Act
        var registrationId = await useCase.ExecuteAsync(
            "Outro Nome",
            "lucas@email.com",
            "61888888888");

        // Assert
        Assert.Equal(originalId, registrationId);
        Assert.Same(existingRegistration, _pendingRepository.Registration);

        Assert.Equal(originalId, existingRegistration.Id);
        Assert.Equal(originalCreatedAt, existingRegistration.CreatedAt);

        Assert.Equal("hash:123456", existingRegistration.CodeHash);
        Assert.Equal(1, existingRegistration.ResendCount);
        Assert.Equal(0, existingRegistration.FailedAttempts);

        // O cadastro original é reutilizado.
        Assert.Equal("Lucas Domingos", existingRegistration.FullName);
        Assert.Equal("61999999999", existingRegistration.PhoneNumber);

        Assert.Equal(0, _pendingRepository.AddCount);
        Assert.Equal(1, _pendingRepository.SaveChangesCount);
        Assert.Equal(1, _emailService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenResendCooldownHasNotElapsed_ShouldThrowException()
    {
        // Arrange
        var existingRegistration = CreateExistingRegistration();

        _pendingRepository.SetRegistration(existingRegistration);

        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                "Lucas Domingos",
                "lucas@email.com",
                "61999999999"));

        // Assert
        Assert.Equal(
            "Aguarde antes de solicitar um novo código.",
            exception.Message);

        Assert.Equal(0, existingRegistration.ResendCount);
        Assert.Equal(0, _pendingRepository.SaveChangesCount);
        Assert.Equal(0, _emailService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenResendLimitHasBeenReached_ShouldThrowException()
    {
        // Arrange
        var existingRegistration = CreateExistingRegistration();

        SetPrivateProperty(
            existingRegistration,
            nameof(PendingEmployeeRegistration.ResendCount),
            PendingEmployeeRegistration.MaxResends);

        SetPrivateProperty(
            existingRegistration,
            nameof(PendingEmployeeRegistration.LastCodeSentAt),
            DateTime.UtcNow.AddSeconds(-61));

        _pendingRepository.SetRegistration(existingRegistration);

        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                "Lucas Domingos",
                "lucas@email.com",
                "61999999999"));

        // Assert
        Assert.Equal(
            "O limite de reenvios de código foi atingido.",
            exception.Message);

        Assert.Equal(0, _pendingRepository.SaveChangesCount);
        Assert.Equal(0, _emailService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPendingRegistrationIsExpired_ShouldAllowRenewal()
    {
        // Arrange
        var existingRegistration = CreateExistingRegistration();

        SetPrivateProperty(
            existingRegistration,
            nameof(PendingEmployeeRegistration.ExpiresAt),
            DateTime.UtcNow.AddMinutes(-1));

        SetPrivateProperty(
            existingRegistration,
            nameof(PendingEmployeeRegistration.LastCodeSentAt),
            DateTime.UtcNow.AddSeconds(-61));

        _pendingRepository.SetRegistration(existingRegistration);

        var useCase = CreateUseCase();

        // Act
        var registrationId = await useCase.ExecuteAsync(
            "Lucas Domingos",
            "lucas@email.com",
            "61999999999");

        // Assert
        Assert.Equal(existingRegistration.Id, registrationId);
        Assert.True(existingRegistration.ExpiresAt > DateTime.UtcNow);
        Assert.Equal("hash:123456", existingRegistration.CodeHash);
        Assert.Equal(1, existingRegistration.ResendCount);
        Assert.Equal(1, _emailService.SendCount);
        Assert.Equal(1, _pendingRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPendingRegistrationIsAlreadyVerified_ShouldThrowException()
    {
        // Arrange
        var existingRegistration = CreateExistingRegistration();

        existingRegistration.MarkAsVerified(DateTime.UtcNow);

        _pendingRepository.SetRegistration(existingRegistration);

        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                "Lucas Domingos",
                "lucas@email.com",
                "61999999999"));

        // Assert
        Assert.Equal(
            "Esta verificação já foi concluída.",
            exception.Message);

        Assert.Equal(0, _pendingRepository.SaveChangesCount);
        Assert.Equal(0, _emailService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailSendingFails_ShouldPropagateExceptionAfterSaving()
    {
        // Arrange
        _emailService.ExceptionToThrow =
            new InvalidOperationException("Falha no envio do e-mail.");

        var useCase = CreateUseCase();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                "Lucas Domingos",
                "lucas@email.com",
                "61999999999"));

        // Assert
        Assert.Equal("Falha no envio do e-mail.", exception.Message);

        Assert.NotNull(_pendingRepository.Registration);
        Assert.Equal(1, _pendingRepository.AddCount);
        Assert.Equal(1, _pendingRepository.SaveChangesCount);
        Assert.Equal(1, _emailService.SendCount);
    }

    private static PendingEmployeeRegistration CreateExistingRegistration()
    {
        return new PendingEmployeeRegistration(
            "Lucas Domingos",
            "lucas@email.com",
            "61999999999",
            "old-hash",
            DateTime.UtcNow.AddMinutes(5));
    }

    private static void SetPrivateProperty<T>(
        object target,
        string propertyName,
        T value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(property);

        property!.SetValue(target, value);
    }

    // Fakes das dependências

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employees = [];

        public void AddEmployee(Employee employee)
        {
            _employees.Add(employee);
        }

        public Task AddAsync(
            Employee employee,
            CancellationToken cancellationToken = default)
        {
            _employees.Add(employee);
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _employees.FirstOrDefault(employee => employee.Id == id));
        }

        public Task<Employee?> GetByPersonalEmailAsync(
            string personalEmail,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = personalEmail.Trim().ToLowerInvariant();

            return Task.FromResult(
                _employees.FirstOrDefault(
                    employee => employee.PersonalEmail == normalizedEmail));
        }

        public Task<IReadOnlyList<Employee>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Employee>>(_employees);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePendingEmployeeRegistrationRepository
        : IPendingEmployeeRegistrationRepository
    {
        public PendingEmployeeRegistration? Registration { get; private set; }

        public int AddCount { get; private set; }

        public int SaveChangesCount { get; private set; }

        public void SetRegistration(PendingEmployeeRegistration registration)
        {
            Registration = registration;
        }

        public Task AddAsync(
            PendingEmployeeRegistration registration,
            CancellationToken cancellationToken = default)
        {
            Registration = registration;
            AddCount++;

            return Task.CompletedTask;
        }

        public Task<PendingEmployeeRegistration?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Registration?.Id == id ? Registration : null);
        }

        public Task<PendingEmployeeRegistration?> GetByPersonalEmailAsync(
            string personalEmail,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = personalEmail.Trim().ToLowerInvariant();

            return Task.FromResult(
                Registration?.PersonalEmail == normalizedEmail
                    ? Registration
                    : null);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeVerificationCodeService
        : IVerificationCodeService
    {
        public int GenerateCount { get; private set; }

        public string GenerateCode()
        {
            GenerateCount++;
            return "123456";
        }

        public string HashCode(string code)
        {
            return $"hash:{code}";
        }

        public bool VerifyCode(string code, string codeHash)
        {
            return codeHash == HashCode(code);
        }
    }

    private sealed class FakeEmailService : IEmailService
    {
        public int SendCount { get; private set; }

        public string? LastEmail { get; private set; }

        public string? LastVerificationCode { get; private set; }

        public Exception? ExceptionToThrow { get; set; }

        public Task SendEmailVerificationCodeAsync(
            string email,
            string verificationCode,
            CancellationToken cancellationToken = default)
        {
            SendCount++;
            LastEmail = email;
            LastVerificationCode = verificationCode;

            if (ExceptionToThrow is not null)
                return Task.FromException(ExceptionToThrow);

            return Task.CompletedTask;
        }
    }
}
