
using CentralITAssistant.API.DTOs.Employees;
using CentralITAssistant.Application.UseCases.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CentralITAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    private readonly RegisterEmployeeUseCase _registerEmployeeUseCase;
    private readonly VerifyEmployeeEmailUseCase _verifyEmployeeEmailUseCase;
    private readonly ResendEmployeeVerificationCodeUseCase _resendEmployeeVerificationCodeUseCase;

    public EmployeesController(
        RegisterEmployeeUseCase registerEmployeeUseCase,
        VerifyEmployeeEmailUseCase verifyEmployeeEmailUseCase,
        ResendEmployeeVerificationCodeUseCase resendEmployeeVerificationCodeUseCase)
    {
        _registerEmployeeUseCase = registerEmployeeUseCase;
        _verifyEmployeeEmailUseCase = verifyEmployeeEmailUseCase;
        _resendEmployeeVerificationCodeUseCase = resendEmployeeVerificationCodeUseCase;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterEmployeeResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterEmployeeResponse>> Register(
        [FromBody] RegisterEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pendingRegistrationId = await _registerEmployeeUseCase.ExecuteAsync(
                request.FullName,
                request.PersonalEmail,
                request.PhoneNumber,
                cancellationToken);

            var response = new RegisterEmployeeResponse
            {
                PendingRegistrationId = pendingRegistrationId,
                Message = "Cadastro iniciado. Enviamos um código de verificação para seu e-mail."
            };

            return Accepted(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmployeeEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerifyEmployeeEmailResponse>> VerifyEmail(
        [FromBody] VerifyEmployeeEmailRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = await _verifyEmployeeEmailUseCase.ExecuteAsync(
                request.PendingRegistrationId,
                request.VerificationCode,
                cancellationToken);

            var response = new VerifyEmployeeEmailResponse
            {
                EmployeeId = employeeId,
                Message = "E-mail verificado e cadastro concluído com sucesso."
            };

            return Ok(response);
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
    [ProducesResponseType(typeof(ResendEmployeeVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResendEmployeeVerificationResponse>> ResendVerification(
        [FromBody] ResendEmployeeVerificationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pendingRegistrationId = await _resendEmployeeVerificationCodeUseCase.ExecuteAsync(
                request.PendingRegistrationId,
                cancellationToken);

            var response = new ResendEmployeeVerificationResponse
            {
                PendingRegistrationId = pendingRegistrationId,
                Message = "Um novo código de verificação foi enviado para seu e-mail."
            };

            return Ok(response);
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
