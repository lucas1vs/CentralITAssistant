
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.UseCases.WhatsApp;

public class VerifyWhatsAppNumberUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPendingWhatsAppVerificationRepository _verificationRepository;
    private readonly IVerificationCodeService _verificationCodeService;

    public VerifyWhatsAppNumberUseCase(
        IEmployeeRepository employeeRepository,
        IPendingWhatsAppVerificationRepository verificationRepository,
        IVerificationCodeService verificationCodeService)
    {
        _employeeRepository = employeeRepository;
        _verificationRepository = verificationRepository;
        _verificationCodeService = verificationCodeService;
    }

    public async Task<Guid> ExecuteAsync(
        Guid verificationId,
        string code)
    {
        if (verificationId == Guid.Empty)
            throw new ArgumentException(
                "A identificação da verificação é obrigatória.",
                nameof(verificationId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "O código de verificação é obrigatório.",
                nameof(code));

        var verification =
            await _verificationRepository.GetByIdAsync(verificationId);

        if (verification is null)
            throw new KeyNotFoundException(
                "Verificação não encontrada.");

        var now = DateTime.UtcNow;

        if (!verification.CanAttemptVerification(now))
            throw new InvalidOperationException(
                "Esta verificação expirou, foi cancelada ou atingiu o limite de tentativas.");

        var isValidCode = _verificationCodeService.VerifyCode(
            code,
            verification.CodeHash);

        if (!isValidCode)
        {
            verification.RegisterFailedAttempt(now);

            await _verificationRepository.SaveChangesAsync();

            throw new InvalidOperationException(
                "Código de verificação inválido.");
        }

        var employee =
            await _employeeRepository.GetByIdWithWhatsAppContactsAsync(
                verification.EmployeeId);

        if (employee is null)
            throw new KeyNotFoundException(
                "Funcionário não encontrado.");

        if (!employee.IsActive)
            throw new InvalidOperationException(
                "Funcionário inativo não pode verificar um número.");

        var currentContact = employee.WhatsAppContacts
            .SingleOrDefault(contact => contact.IsActive);

        if (currentContact is not null &&
            currentContact.PhoneNumber == verification.PhoneNumber)
        {
            if (currentContact.IsVerified)
                throw new InvalidOperationException(
                    "Este número já está verificado.");

            currentContact.Verify();
        }
        else
        {
            var newContact = new WhatsAppContact(
                verification.PhoneNumber);

            newContact.Verify();

            if (currentContact is null)
                employee.AssociateWhatsAppContact(newContact);
            else
                employee.ReplaceWhatsAppContact(newContact);
        }

        verification.MarkAsVerified(now);

        await _verificationRepository.SaveChangesAsync();

        return employee.Id;
    }
}
