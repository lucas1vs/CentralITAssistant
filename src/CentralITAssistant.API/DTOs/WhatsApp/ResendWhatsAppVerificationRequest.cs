using System.ComponentModel.DataAnnotations;

namespace CentralITAssistant.API.DTOs.WhatsApp;

public sealed class ResendWhatsAppVerificationRequest
{
    [Required(ErrorMessage = "O identificador da verificação é obrigatório.")]
    public Guid VerificationId { get; init; }
}