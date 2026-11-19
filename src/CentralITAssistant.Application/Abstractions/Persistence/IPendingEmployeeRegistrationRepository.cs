
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.Abstractions.Persistence;

public interface IPendingEmployeeRegistrationRepository
{
    Task AddAsync(
        PendingEmployeeRegistration registration,
        CancellationToken cancellationToken = default);

    Task<PendingEmployeeRegistration?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PendingEmployeeRegistration?> GetByPersonalEmailAsync(
        string personalEmail,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
