
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.UseCases.Employees;

public class RegisterEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPendingEmployeeRegistrationRepository _pendingRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IEmailService _emailService;

    public RegisterEmployeeUseCase(
        IEmployeeRepository employeeRepository,
        IPendingEmployeeRegistrationRepository pendingRepository,
        IVerificationCodeService verificationCodeService,
        IEmailService emailService)
    {
        _employeeRepository = employeeRepository;
        _pendingRepository = pendingRepository;
        _verificationCodeService = verificationCodeService;
        _emailService = emailService;
    }

    public async Task<Guid> ExecuteAsync(
        string fullName,
        string personalEmail,
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = personalEmail.Trim().ToLowerInvariant();

        var existingEmployee =
            await _employeeRepository.GetByPersonalEmailAsync(
                normalizedEmail, cancellationToken);

        if (existingEmployee is not null)
        {
            throw new InvalidOperationException(
                "Já existe um funcionário cadastrado com este e-mail.");
        }

        var existingRegistration =
            await _pendingRepository.GetByPersonalEmailAsync(
                normalizedEmail, cancellationToken);

        var verificationCode = _verificationCodeService.GenerateCode();
        var codeHash = _verificationCodeService.HashCode(verificationCode);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(5);

        Guid registrationId;

        if (existingRegistration is not null)
        {
            existingRegistration.RenewVerification(
                codeHash,
                now,
                expiresAt);

            registrationId = existingRegistration.Id;
        }
        else
        {
            var registration = new PendingEmployeeRegistration(
                fullName,
                normalizedEmail,
                phoneNumber,
                codeHash,
                expiresAt);

            await _pendingRepository.AddAsync(
                registration,
                cancellationToken);

            registrationId = registration.Id;
        }

        await _pendingRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationCodeAsync(
            normalizedEmail,
            verificationCode,
            cancellationToken);

        return registrationId;
    }
}
