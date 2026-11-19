
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.UseCases.Employees;

public class VerifyEmployeeEmailUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPendingEmployeeRegistrationRepository _pendingRepository;
    private readonly IVerificationCodeService _verificationCodeService;

    public VerifyEmployeeEmailUseCase(
        IEmployeeRepository employeeRepository,
        IPendingEmployeeRegistrationRepository pendingRepository,
        IVerificationCodeService verificationCodeService)
    {
        _employeeRepository = employeeRepository;
        _pendingRepository = pendingRepository;
        _verificationCodeService = verificationCodeService;
    }

    public async Task<Guid> ExecuteAsync(
        Guid pendingRegistrationId,
        string verificationCode,
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

        var now = DateTime.UtcNow;

        if (!registration.CanAttemptVerification(now))
        {
            throw new InvalidOperationException(
                "O cadastro não pode mais ser verificado. O código pode ter expirado ou excedido o limite de tentativas.");
        }

        var isValid = _verificationCodeService.VerifyCode(
            verificationCode,
            registration.CodeHash);

        if (!isValid)
        {
            registration.RegisterFailedAttempt();

            await _pendingRepository.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException(
                "Código de verificação inválido.");
        }

        registration.MarkAsVerified(now);

        var employee = new Employee(
            registration.FullName,
            registration.PersonalEmail);

        var whatsAppContact = new WhatsAppContact(
            registration.PhoneNumber);

        employee.AssociateWhatsAppContact(whatsAppContact);

        await _employeeRepository.AddAsync(
            employee,
            cancellationToken);

        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
