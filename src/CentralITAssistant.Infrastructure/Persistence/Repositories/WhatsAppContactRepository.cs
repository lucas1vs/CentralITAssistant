using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Infrastructure.Persistence.Repositories;

public class WhatsAppContactRepository : IWhatsAppContactRepository
{
    private readonly AppDbContext _context;

    public WhatsAppContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByPhoneNumberForAnotherEmployeeAsync(
    string phoneNumber,
    Guid employeeId)
    {
        return await _context.WhatsAppContacts
            .AnyAsync(contact =>
                contact.PhoneNumber == phoneNumber &&
                contact.EmployeeId != employeeId);
    }
}