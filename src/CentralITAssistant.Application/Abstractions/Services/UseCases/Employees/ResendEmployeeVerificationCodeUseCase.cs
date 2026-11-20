
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;

namespace CentralITAssistant.Application.UseCases.Employees;

public class ResendEmployeeVerificationCodeUseCase
{
    private readonly IPendingEmployeeRegistrationRepository _pendingRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IEmailService _emailService;

    public ResendEmployeeVerificationCodeUseCase(
        IPendingEmployeeRegistrationRepository pendingRepository,
        IVerificationCodeService verificationCodeService,
        IEmailService emailService)
    {
        _pendingRepository = pendingRepository;
        _verificationCodeService = verificationCodeService;
        _emailService = emailService;
    }

    public async Task<Guid> ExecuteAsync(
        Guid pendingRegistrationId,
        CancellationToken cancellationToken = default)
    {
        var registration = await _pendingRepository.GetByIdAsync(
            pendingRegistrationId,
            cancellationToken);

        if (registration is null)
        {
            throw new KeyNotFoundException(
                "Cadastro pendente não encontrado.");
        }

        var verificationCode = _verificationCodeService.GenerateCode();
        var codeHash = _verificationCodeService.HashCode(verificationCode);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(5);

        registration.RenewVerification(
            codeHash,
            now,
            expiresAt);

        await _pendingRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationCodeAsync(
            registration.PersonalEmail,
            verificationCode,
            cancellationToken);

        return registration.Id;
    }
}
