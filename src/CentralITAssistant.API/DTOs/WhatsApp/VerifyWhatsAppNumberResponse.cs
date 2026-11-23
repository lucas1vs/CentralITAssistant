namespace CentralITAssistant.API.DTOs.WhatsApp;

public sealed class VerifyWhatsAppNumberResponse
{
    public Guid EmployeeId { get; init; }
    public string Message { get; init; } = string.Empty;
}