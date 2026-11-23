using System.ComponentModel.DataAnnotations;

namespace CentralITAssistant.API.DTOs.WhatsApp;

public sealed class VerifyWhatsAppNumberRequest
{
    [Required(ErrorMessage = "O identificador da verificação é obrigatório.")]
    public Guid VerificationId { get; init; }

    [Required(ErrorMessage = "O código de verificação é obrigatório.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "O código deve conter exatamente 6 dígitos.")]
    public string VerificationCode { get; init; } = string.Empty;
}