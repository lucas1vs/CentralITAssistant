
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.Abstractions.Persistence;

public interface IPendingWhatsAppVerificationRepository
{
    Task<PendingWhatsAppVerification?> GetByIdAsync(Guid id);

    Task<PendingWhatsAppVerification?> GetActiveByEmployeeIdAsync(
        Guid employeeId);

    Task AddAsync(PendingWhatsAppVerification verification);

    Task SaveChangesAsync();
}
