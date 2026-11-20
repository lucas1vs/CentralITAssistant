
namespace CentralITAssistant.API.DTOs.Employees;

public sealed class RegisterEmployeeResponse
{
    public Guid PendingRegistrationId { get; init; }

    public string Message { get; init; } = string.Empty;
}
