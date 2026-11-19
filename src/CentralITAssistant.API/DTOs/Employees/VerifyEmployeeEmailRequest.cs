
using System.ComponentModel.DataAnnotations;

namespace CentralITAssistant.API.DTOs.Employees;

public class VerifyEmployeeEmailRequest
{
    [Required(ErrorMessage = "O identificador do cadastro é obrigatório.")]
    public Guid PendingRegistrationId { get; set; }

    [Required(ErrorMessage = "O código de verificação é obrigatório.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "O código deve conter exatamente 6 dígitos.")]
    public string VerificationCode { get; set; } = string.Empty;
}
