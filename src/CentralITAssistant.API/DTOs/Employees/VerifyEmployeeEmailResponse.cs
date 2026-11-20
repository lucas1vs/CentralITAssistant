
namespace CentralITAssistant.API.DTOs.Employees;

public sealed class VerifyEmployeeEmailResponse
{
    public Guid EmployeeId { get; init; }

    public string Message { get; init; } = string.Empty;
}
