
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.Employees;
using CentralITAssistant.Domain.Entities;
using Xunit;

namespace CentralITAssistant.Tests.Unit.Application.UseCases.Employees;

public class ResendEmployeeVerificationCodeUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldResendCode_WhenRegistrationCanBeResent()
    {
        // Arrange
        var registration = CreateRegistration();
        SetLastCodeSentAt(registration, DateTime.UtcNow.AddMinutes(-2));

        var repository = new FakePendingEmployeeRegistrationRepository(registration);
        var codeService = new FakeVerificationCodeService();
        var emailService = new FakeEmailService();

        var useCase = CreateUseCase(repository, codeService, emailService);

        // Act
        var result = await useCase.ExecuteAsync(registration.Id);

        // Assert
        Assert.Equal(registration.Id, result);
        Assert.Equal(1, repository.SaveChangesCount);
        Assert.Equal("123456", emailService.LastVerificationCode);
        Assert.Equal(registration.PersonalEmail, emailService.LastEmail);
        Assert.Equal("hash-123456", registration.CodeHash);
        Assert.Equal(1, registration.ResendCount);
        Assert.Equal(0, registration.FailedAttempts);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenRegistrationDoesNotExist()
    {
        // Arrange
        var repository = new FakePendingEmployeeRegistrationRepository(null);
        var codeService = new FakeVerificationCodeService();
        var emailService = new FakeEmailService();

        var useCase = CreateUseCase(repository, codeService, emailService);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid()));

        // Assert
        Assert.Equal("Cadastro pendente não encontrado.", exception.Message);
        Assert.Equal(0, repository.SaveChangesCount);
        Assert.Null(emailService.LastEmail);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenRegistrationIsVerified()
    {
        // Arrange
        var registration = CreateRegistration();
        registration.MarkAsVerified(DateTime.UtcNow);

        var repository = new FakePendingEmployeeRegistrationRepository(registration);

        var useCase = CreateUseCase(
            repository,
            new FakeVerificationCodeService(),
            new FakeEmailService());

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(registration.Id));

        // Assert
        Assert.Equal(0, repository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenResendCooldownHasNotElapsed()
    {
        // Arrange
        var registration = CreateRegistration();

        var repository = new FakePendingEmployeeRegistrationRepository(registration);
        var emailService = new FakeEmailService();

        var useCase = CreateUseCase(
            repository,
            new FakeVerificationCodeService(),
            emailService);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(registration.Id));

        // Assert
        Assert.Equal(0, repository.SaveChangesCount);
        Assert.Null(emailService.LastEmail);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenResendLimitIsReached()
    {
        // Arrange
        var registration = CreateRegistration();

        var now = DateTime.UtcNow.AddMinutes(-10);

        // Coloca o último envio suficientemente no passado
        // para que o primeiro reenvio respeite o cooldown.
        SetLastCodeSentAt(registration, now.AddMinutes(-2));

        for (var i = 0; i < PendingEmployeeRegistration.MaxResends; i++)
        {
            now = now.AddMinutes(2);

            registration.RenewVerification(
                $"hash-{i}",
                now,
                now.AddMinutes(5));
        }

        // Garante que o cooldown também esteja concluído
        // quando o caso de uso tentar realizar o próximo reenvio.
        SetLastCodeSentAt(registration, DateTime.UtcNow.AddMinutes(-2));

        var repository = new FakePendingEmployeeRegistrationRepository(registration);
        var emailService = new FakeEmailService();

        var useCase = CreateUseCase(
            repository,
            new FakeVerificationCodeService(),
            emailService);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(registration.Id));

        // Assert
        Assert.Equal(PendingEmployeeRegistration.MaxResends, registration.ResendCount);
        Assert.Equal(0, repository.SaveChangesCount);
        Assert.Null(emailService.LastEmail);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenEmailSendingFails()
    {
        // Arrange
        var registration = CreateRegistration();
        SetLastCodeSentAt(registration, DateTime.UtcNow.AddMinutes(-2));

        var repository = new FakePendingEmployeeRegistrationRepository(registration);

        var emailService = new FakeEmailService
        {
            ShouldThrowException = true
        };

        var useCase = CreateUseCase(
            repository,
            new FakeVerificationCodeService(),
            emailService);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(registration.Id));

        // Assert
        Assert.Equal(1, repository.SaveChangesCount);
        Assert.Equal(1, registration.ResendCount);
    }

    private static ResendEmployeeVerificationCodeUseCase CreateUseCase(
        IPendingEmployeeRegistrationRepository repository,
        IVerificationCodeService codeService,
        IEmailService emailService)
    {
        return new ResendEmployeeVerificationCodeUseCase(
            repository,
            codeService,
            emailService);
    }

    private static PendingEmployeeRegistration CreateRegistration()
    {
        return new PendingEmployeeRegistration(
            "João da Silva",
            "joao@gmail.com",
            "5561999999999",
            "old-hash",
            DateTime.UtcNow.AddMinutes(5));
    }

    private static void SetLastCodeSentAt(
        PendingEmployeeRegistration registration,
        DateTime dateTime)
    {
        typeof(PendingEmployeeRegistration)
            .GetProperty(nameof(PendingEmployeeRegistration.LastCodeSentAt))!
            .SetValue(registration, dateTime);
    }

    private sealed class FakePendingEmployeeRegistrationRepository
        : IPendingEmployeeRegistrationRepository
    {
        private readonly PendingEmployeeRegistration? _registration;

        public int SaveChangesCount { get; private set; }

        public FakePendingEmployeeRegistrationRepository(
            PendingEmployeeRegistration? registration)
        {
            _registration = registration;
        }

        public Task AddAsync(
            PendingEmployeeRegistration registration,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PendingEmployeeRegistration?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _registration?.Id == id ? _registration : null);
        }

        public Task<PendingEmployeeRegistration?> GetByPersonalEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _registration?.PersonalEmail == email ? _registration : null);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeVerificationCodeService : IVerificationCodeService
    {
        public string GenerateCode() => "123456";

        public string HashCode(string code) => $"hash-{code}";

        public bool VerifyCode(string code, string codeHash)
        {
            return codeHash == HashCode(code);
        }
    }

    private sealed class FakeEmailService : IEmailService
    {
        public string? LastEmail { get; private set; }

        public string? LastVerificationCode { get; private set; }

        public bool ShouldThrowException { get; init; }

        public Task SendEmailVerificationCodeAsync(
            string email,
            string verificationCode,
            CancellationToken cancellationToken = default)
        {
            LastEmail = email;
            LastVerificationCode = verificationCode;

            if (ShouldThrowException)
            {
                throw new InvalidOperationException(
                    "Falha simulada no envio do e-mail.");
            }

            return Task.CompletedTask;
        }
    }
}
