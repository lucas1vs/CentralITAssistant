using System.ComponentModel.DataAnnotations;

namespace CentralITAssistant.API.DTOs.Employees;

public class ResendEmployeeVerificationRequest
{
    [Required]
    public Guid PendingRegistrationId { get; init; }
}