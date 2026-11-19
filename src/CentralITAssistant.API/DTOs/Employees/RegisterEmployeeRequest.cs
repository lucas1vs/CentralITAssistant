
using System.ComponentModel.DataAnnotations;

namespace CentralITAssistant.API.DTOs.Employees;

public class RegisterEmployeeRequest
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail pessoal é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(256)]
    public string PersonalEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número do WhatsApp é obrigatório.")]
    public string PhoneNumber { get; set; } = string.Empty;
}
