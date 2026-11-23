namespace CentralITAssistant.API.DTOs.WhatsApp;

public sealed class ResendWhatsAppVerificationResponse
{
    public Guid VerificationId { get; init; }
    public string Message { get; init; } = string.Empty;
}