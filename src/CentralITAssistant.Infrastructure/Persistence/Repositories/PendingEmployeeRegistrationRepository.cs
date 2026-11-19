
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Infrastructure.Persistence.Repositories;

public class PendingEmployeeRegistrationRepository
    : IPendingEmployeeRegistrationRepository
{
    private readonly AppDbContext _context;

    public PendingEmployeeRegistrationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        PendingEmployeeRegistration registration,
        CancellationToken cancellationToken = default)
    {
        await _context.PendingEmployeeRegistrations.AddAsync(
            registration,
            cancellationToken);
    }

    public async Task<PendingEmployeeRegistration?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PendingEmployeeRegistrations
            .FirstOrDefaultAsync(
                registration => registration.Id == id,
                cancellationToken);
    }

    public async Task<PendingEmployeeRegistration?> GetByPersonalEmailAsync(
        string personalEmail,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = personalEmail.Trim().ToLowerInvariant();

        return await _context.PendingEmployeeRegistrations
            .FirstOrDefaultAsync(
                registration => registration.PersonalEmail == normalizedEmail,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
