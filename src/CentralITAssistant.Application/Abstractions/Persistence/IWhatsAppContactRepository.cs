namespace CentralITAssistant.Application.Abstractions.Persistence;

public interface IWhatsAppContactRepository
{
    Task<bool> ExistsByPhoneNumberForAnotherEmployeeAsync(
        string phoneNumber,
        Guid employeeId);
}