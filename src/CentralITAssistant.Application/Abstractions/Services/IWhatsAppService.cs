
namespace CentralITAssistant.Application.Abstractions.Services;

public interface IWhatsAppService
{
    Task SendVerificationCodeAsync(
        string phoneNumber,
        string code);
}
