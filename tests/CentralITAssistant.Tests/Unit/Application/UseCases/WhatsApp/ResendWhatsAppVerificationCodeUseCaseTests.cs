
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.WhatsApp;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Application.UseCases.WhatsApp;

public class ResendWhatsAppVerificationCodeUseCaseTests
{
    private const string ValidCode = "123456";

    private static PendingWhatsAppVerification CreateVerification(
        DateTime? sentAt = null,
        int resendCount = 0)
    {
        var now = sentAt ?? DateTime.UtcNow;

        return new PendingWhatsAppVerification(
            Guid.NewGuid(),
            "+5561999999999",
            "hash:old-code",
            now.AddMinutes(5),
            now,
            resendCount);
    }

    private static ResendWhatsAppVerificationCodeUseCase CreateUseCase(
        FakePendingWhatsAppVerificationRepository? verificationRepository = null,
        FakeVerificationCodeService? codeService = null,
        FakeWhatsAppService? whatsAppService = null)
    {
        return new ResendWhatsAppVerificationCodeUseCase(
            verificationRepository ?? new FakePendingWhatsAppVerificationRepository(),
            codeService ?? new FakeVerificationCodeService(),
            whatsAppService ?? new FakeWhatsAppService());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDataIsValid_ShouldRenewVerificationAndSendNewCode()
    {
        var verification = CreateVerification(
            sentAt: DateTime.UtcNow.AddSeconds(-61));

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var codeService = new FakeVerificationCodeService();
        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            codeService,
            whatsAppService);

        var originalHash = verification.CodeHash;
        var originalExpiration = verification.ExpiresAt;

        var result = await useCase.ExecuteAsync(verification.Id);

        Assert.Equal(verification.Id, result);

        Assert.Equal("hash:123456", verification.CodeHash);
        Assert.NotEqual(originalHash, verification.CodeHash);

        Assert.True(verification.ExpiresAt > originalExpiration);
        Assert.Equal(1, verification.ResendCount);
        Assert.Equal(0, verification.FailedAttempts);

        Assert.Equal(1, verificationRepository.SaveChangesCount);

        Assert.Equal("+5561999999999", whatsAppService.LastPhoneNumber);
        Assert.Equal(ValidCode, whatsAppService.LastCode);
        Assert.Equal(1, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationDoesNotExist_ShouldThrowException()
    {
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository();

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid()));

        Assert.Equal("Verificação não encontrada.", exception.Message);

        Assert.Equal(0, verificationRepository.SaveChangesCount);
        Assert.Equal(0, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIdIsEmpty_ShouldThrowException()
    {
        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository();

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(Guid.Empty));

        Assert.Equal("verificationId", exception.ParamName);

        Assert.Equal(0, verificationRepository.SaveChangesCount);
        Assert.Equal(0, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCooldownIsActive_ShouldThrowException()
    {
        var verification = CreateVerification();

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var whatsAppService = new FakeWhatsAppService();

        var originalHash = verification.CodeHash;
        var originalResendCount = verification.ResendCount;

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(verification.Id));

        Assert.Equal(
            "Não é possível reenviar o código. Verifique o intervalo de reenvio e o limite de tentativas.",
            exception.Message);

        Assert.Equal(originalHash, verification.CodeHash);
        Assert.Equal(originalResendCount, verification.ResendCount);

        Assert.Equal(0, verificationRepository.SaveChangesCount);
        Assert.Equal(0, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMaximumResendsReached_ShouldThrowException()
    {
        var verification = CreateVerification(
            sentAt: DateTime.UtcNow.AddSeconds(-61),
            resendCount: PendingWhatsAppVerification.MaxResends);

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(verification.Id));

        Assert.Equal(
            "Não é possível reenviar o código. Verifique o intervalo de reenvio e o limite de tentativas.",
            exception.Message);

        Assert.Equal(
            PendingWhatsAppVerification.MaxResends,
            verification.ResendCount);

        Assert.Equal(0, verificationRepository.SaveChangesCount);
        Assert.Equal(0, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIsCancelled_ShouldThrowException()
    {
        var verification = CreateVerification(
            sentAt: DateTime.UtcNow.AddSeconds(-61));

        verification.Cancel();

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(verification.Id));

        Assert.Equal(
            "Não é possível reenviar o código. Verifique o intervalo de reenvio e o limite de tentativas.",
            exception.Message);

        Assert.True(verification.IsCancelled);
        Assert.Equal(0, verificationRepository.SaveChangesCount);
        Assert.Equal(0, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIsAlreadyCompleted_ShouldThrowException()
    {
        var verification = CreateVerification(
            sentAt: DateTime.UtcNow.AddSeconds(-61));

        verification.MarkAsVerified(DateTime.UtcNow);

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(verification.Id));

        Assert.Equal(
            "Não é possível reenviar o código. Verifique o intervalo de reenvio e o limite de tentativas.",
            exception.Message);

        Assert.True(verification.IsVerified);
        Assert.Equal(0, verificationRepository.SaveChangesCount);
        Assert.Equal(0, whatsAppService.SendCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVerificationIsExpired_ShouldRenewAndSendNewCode()
    {
        var sentAt = DateTime.UtcNow.AddMinutes(-10);

        var verification = new PendingWhatsAppVerification(
            Guid.NewGuid(),
            "+5561999999999",
            "hash:old-code",
            sentAt.AddMinutes(5),
            sentAt);

        var verificationRepository =
            new FakePendingWhatsAppVerificationRepository(verification);

        var whatsAppService = new FakeWhatsAppService();

        var useCase = CreateUseCase(
            verificationRepository,
            whatsAppService: whatsAppService);

        var result = await useCase.ExecuteAsync(verification.Id);

        Assert.Equal(verification.Id, result);
        Assert.False(verification.IsExpired(DateTime.UtcNow));

        Assert.Equal(1, verification.ResendCount);
        Assert.Equal(1, verificationRepository.SaveChangesCount);
        Assert.Equal(1, whatsAppService.SendCount);
    }

    // Fakes

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
