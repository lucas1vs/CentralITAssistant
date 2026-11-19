namespace CentralITAssistant.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendEmailVerificationCodeAsync(
        string email,
        string verificationCode,
        CancellationToken cancellationToken = default);
}
