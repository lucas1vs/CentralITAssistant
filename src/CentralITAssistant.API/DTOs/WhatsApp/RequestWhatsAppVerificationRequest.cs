
using System.ComponentModel.DataAnnotations;

namespace CentralITAssistant.API.DTOs.WhatsApp;

public sealed class RequestWhatsAppVerificationRequest
{
    public Guid EmployeeId { get; init; }

    [Required(ErrorMessage = "O número de WhatsApp é obrigatório.")]
    public string PhoneNumber { get; init; } = string.Empty;
}
