
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(employee);

        await _context.Employees.AddAsync(
            employee,
            cancellationToken);
    }

    public async Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(
                e => e.Id == id,
                cancellationToken);
    }

    public async Task<Employee?> GetByIdWithWhatsAppContactsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.WhatsAppContacts)
            .FirstOrDefaultAsync(
                e => e.Id == id,
                cancellationToken);
    }

    public async Task<Employee?> GetByPersonalEmailAsync(
        string personalEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(personalEmail);

        var normalizedEmail = personalEmail.Trim().ToLowerInvariant();

        return await _context.Employees
            .FirstOrDefaultAsync(
                e => e.PersonalEmail == normalizedEmail,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
