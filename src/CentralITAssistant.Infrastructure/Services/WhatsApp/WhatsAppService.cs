using CentralITAssistant.Application.Abstractions.Services;

namespace CentralITAssistant.Infrastructure.Services.WhatsAppApp;

public class WhatsAppService : IWhatsAppService
{
    public Task SendVerificationCodeAsync(
        string phoneNumber,
        string code)
    {
        throw new InvalidOperationException(
            "O envio de mensagens pelo WhatsApp ainda não foi configurado.");
    }
}