
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.UseCases.WhatsApp;

public class RequestWhatsAppVerificationUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPendingWhatsAppVerificationRepository _verificationRepository;
    private readonly IWhatsAppContactRepository _whatsAppContactRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IWhatsAppService _whatsAppService;

    public RequestWhatsAppVerificationUseCase(
        IEmployeeRepository employeeRepository,
        IPendingWhatsAppVerificationRepository verificationRepository,
        IWhatsAppContactRepository whatsAppContactRepository,
        IVerificationCodeService verificationCodeService,
        IWhatsAppService whatsAppService)
    {
        _employeeRepository = employeeRepository;
        _verificationRepository = verificationRepository;
        _whatsAppContactRepository = whatsAppContactRepository;
        _verificationCodeService = verificationCodeService;
        _whatsAppService = whatsAppService;
    }

    public async Task<Guid> ExecuteAsync(
        Guid employeeId,
        string phoneNumber)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException(
                "O funcionário é obrigatório.",
                nameof(employeeId));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException(
                "O número de WhatsApp é obrigatório.",
                nameof(phoneNumber));

        var employee = await _employeeRepository.GetByIdAsync(employeeId);

        if (employee is null)
            throw new KeyNotFoundException(
                "Funcionário não encontrado.");

        if (!employee.IsActive)
            throw new InvalidOperationException(
                "Funcionário inativo não pode solicitar verificação.");

        var normalizedPhoneNumber = phoneNumber.Trim();

        var currentContact = employee.WhatsAppContacts
            .SingleOrDefault(contact => contact.IsActive);

        if (currentContact is not null &&
            currentContact.PhoneNumber == normalizedPhoneNumber &&
            currentContact.IsVerified)
        {
            throw new InvalidOperationException(
                "O número informado já está verificado e ativo.");
        }

        var belongsToAnotherEmployee =
            await _whatsAppContactRepository
                .ExistsByPhoneNumberForAnotherEmployeeAsync(
                    normalizedPhoneNumber,
                    employeeId);

        if (belongsToAnotherEmployee)
            throw new InvalidOperationException(
                "Não foi possível utilizar o número informado.");

        var existingVerification =
            await _verificationRepository.GetActiveByEmployeeIdAsync(
                employeeId);

        if (existingVerification is not null)
            existingVerification.Cancel();

        var code = _verificationCodeService.GenerateCode();
        var codeHash = _verificationCodeService.HashCode(code);

        var now = DateTime.UtcNow;

        var verification = new PendingWhatsAppVerification(
            employeeId,
            normalizedPhoneNumber,
            codeHash,
            now.AddMinutes(5),
            now);

        await _verificationRepository.AddAsync(verification);
        await _verificationRepository.SaveChangesAsync();

        await _whatsAppService.SendVerificationCodeAsync(
            normalizedPhoneNumber,
            code);

        return verification.Id;
    }
}
