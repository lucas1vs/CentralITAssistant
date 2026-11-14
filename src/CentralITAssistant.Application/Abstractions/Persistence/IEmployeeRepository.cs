
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Application.Abstractions.Persistence;

public interface IEmployeeRepository
{
    Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetByCorporateEmailAsync(
        string corporateEmail,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Employee>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
