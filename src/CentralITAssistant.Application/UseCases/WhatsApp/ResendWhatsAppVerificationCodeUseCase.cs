using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;

namespace CentralITAssistant.Application.UseCases.WhatsApp;

public class ResendWhatsAppVerificationCodeUseCase
{
    private readonly IPendingWhatsAppVerificationRepository _verificationRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IWhatsAppService _whatsAppService;

    public ResendWhatsAppVerificationCodeUseCase(
        IPendingWhatsAppVerificationRepository verificationRepository,
        IVerificationCodeService verificationCodeService,
        IWhatsAppService whatsAppService)
    {
        _verificationRepository = verificationRepository;
        _verificationCodeService = verificationCodeService;
        _whatsAppService = whatsAppService;
    }

    public async Task<Guid> ExecuteAsync(Guid verificationId)
    {
        if (verificationId == Guid.Empty)
            throw new ArgumentException(
                "A verificação é obrigatória.",
                nameof(verificationId));

        var verification =
            await _verificationRepository.GetByIdAsync(verificationId);

        if (verification is null)
            throw new KeyNotFoundException(
                "Verificação não encontrada.");

        var now = DateTime.UtcNow;

        if (!verification.CanResendCode(now))
            throw new InvalidOperationException(
                "Não é possível reenviar o código. Verifique o intervalo de reenvio e o limite de tentativas.");

        var code = _verificationCodeService.GenerateCode();

        var codeHash = _verificationCodeService.HashCode(code);

        verification.RenewVerification(
            codeHash,
            now.AddMinutes(5),
            now);

        await _verificationRepository.SaveChangesAsync();

        await _whatsAppService.SendVerificationCodeAsync(
            verification.PhoneNumber,
            code);

        return verification.Id;
    }
}