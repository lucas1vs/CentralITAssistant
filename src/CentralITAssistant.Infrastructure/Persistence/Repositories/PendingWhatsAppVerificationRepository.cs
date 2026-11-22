
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Infrastructure.Persistence.Repositories;

public class PendingWhatsAppVerificationRepository
    : IPendingWhatsAppVerificationRepository
{
    private readonly AppDbContext _context;

    public PendingWhatsAppVerificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PendingWhatsAppVerification?> GetByIdAsync(Guid id)
    {
        return await _context.Set<PendingWhatsAppVerification>()
            .FirstOrDefaultAsync(verification => verification.Id == id);
    }

    public async Task<PendingWhatsAppVerification?>
        GetActiveByEmployeeIdAsync(Guid employeeId)
    {
        return await _context.Set<PendingWhatsAppVerification>()
            .FirstOrDefaultAsync(verification =>
                verification.EmployeeId == employeeId &&
                !verification.IsVerified &&
                !verification.IsCancelled);
    }

    public async Task AddAsync(PendingWhatsAppVerification verification)
    {
        await _context.Set<PendingWhatsAppVerification>()
            .AddAsync(verification);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
