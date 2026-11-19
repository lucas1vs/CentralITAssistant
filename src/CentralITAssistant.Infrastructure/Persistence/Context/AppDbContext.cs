
using CentralITAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<PendingEmployeeRegistration> PendingEmployeeRegistrations =>
        Set<PendingEmployeeRegistration>();

    public DbSet<WhatsAppContact> WhatsAppContacts =>
        Set<WhatsAppContact>();

    public DbSet<Conversation> Conversations =>
        Set<Conversation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
