namespace CentralITAssistant.API.DTOs.Employees;

public class ResendEmployeeVerificationResponse
{
    public Guid PendingRegistrationId { get; init; }
    public string Message { get; init; } = string.Empty;
}