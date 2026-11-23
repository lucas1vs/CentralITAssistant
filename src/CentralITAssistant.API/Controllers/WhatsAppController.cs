using CentralITAssistant.API.DTOs.WhatsApp;
using CentralITAssistant.Application.UseCases.WhatsApp;
using Microsoft.AspNetCore.Mvc;

namespace CentralITAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WhatsAppController : ControllerBase
{
    private readonly RequestWhatsAppVerificationUseCase _requestWhatsAppVerificationUseCase;
    private readonly VerifyWhatsAppNumberUseCase _verifyWhatsAppNumberUseCase;
    private readonly ResendWhatsAppVerificationCodeUseCase _resendWhatsAppVerificationCodeUseCase;

    public WhatsAppController(
        RequestWhatsAppVerificationUseCase requestWhatsAppVerificationUseCase,
        VerifyWhatsAppNumberUseCase verifyWhatsAppNumberUseCase,
        ResendWhatsAppVerificationCodeUseCase resendWhatsAppVerificationCodeUseCase)
    {
        _requestWhatsAppVerificationUseCase = requestWhatsAppVerificationUseCase;
        _verifyWhatsAppNumberUseCase = verifyWhatsAppNumberUseCase;
        _resendWhatsAppVerificationCodeUseCase = resendWhatsAppVerificationCodeUseCase;
    }

    [HttpPost("request-verification")]
    [ProducesResponseType(typeof(RequestWhatsAppVerificationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestWhatsAppVerificationResponse>> RequestVerification(
        [FromBody] RequestWhatsAppVerificationRequest request)
    {
        try
        {
            var verificationId = await _requestWhatsAppVerificationUseCase.ExecuteAsync(
                request.EmployeeId,
                request.PhoneNumber);

            var response = new RequestWhatsAppVerificationResponse
            {
                VerificationId = verificationId,
                Message = "Solicitação realizada. Enviamos um código de verificação para seu WhatsApp."
            };

            return Accepted(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyWhatsAppNumberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerifyWhatsAppNumberResponse>> Verify(
        [FromBody] VerifyWhatsAppNumberRequest request)
    {
        try
        {
            var employeeId = await _verifyWhatsAppNumberUseCase.ExecuteAsync(
                request.VerificationId,
                request.VerificationCode);

            var response = new VerifyWhatsAppNumberResponse
            {
                EmployeeId = employeeId,
                Message = "Número de WhatsApp verificado com sucesso."
            };

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(ResendWhatsAppVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResendWhatsAppVerificationResponse>> ResendVerification(
        [FromBody] ResendWhatsAppVerificationRequest request)
    {
        try
        {
            var verificationId = await _resendWhatsAppVerificationCodeUseCase.ExecuteAsync(
                request.VerificationId);

            var response = new ResendWhatsAppVerificationResponse
            {
                VerificationId = verificationId,
                Message = "Um novo código de verificação foi enviado para seu WhatsApp."
            };

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}