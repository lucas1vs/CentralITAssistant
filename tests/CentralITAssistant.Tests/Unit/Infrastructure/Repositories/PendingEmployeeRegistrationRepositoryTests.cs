
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Tests.Unit.Infrastructure.Repositories;

public class PendingEmployeeRegistrationRepositoryTests
{
    private static PendingEmployeeRegistration CreateRegistration(
        string email = "lucas@email.com")
    {
        return new PendingEmployeeRegistration(
            "Lucas Domingos",
            email,
            "5561999999999",
            "test-code-hash",
            DateTime.UtcNow.AddMinutes(5));
    }

    [Fact]
    public async Task AddAsync_ShouldAddRegistrationToContext()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var repository = new PendingEmployeeRegistrationRepository(
            database.Context);

        var registration = CreateRegistration();

        await repository.AddAsync(registration);

        var entry = database.Context.Entry(registration);

        Assert.Equal(EntityState.Added, entry.State);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRegistration_WhenExists()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var repository = new PendingEmployeeRegistrationRepository(
            database.Context);

        var registration = CreateRegistration();

        await repository.AddAsync(registration);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(registration.Id);

        Assert.NotNull(result);
        Assert.Equal(registration.Id, result.Id);
        Assert.Equal(registration.PersonalEmail, result.PersonalEmail);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var repository = new PendingEmployeeRegistrationRepository(
            database.Context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPersonalEmailAsync_ShouldReturnRegistration_WhenExists()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var repository = new PendingEmployeeRegistrationRepository(
            database.Context);

        var registration = CreateRegistration();

        await repository.AddAsync(registration);
        await repository.SaveChangesAsync();

        var result = await repository.GetByPersonalEmailAsync(
            "LUCAS@EMAIL.COM");

        Assert.NotNull(result);
        Assert.Equal(registration.Id, result.Id);
        Assert.Equal("lucas@email.com", result.PersonalEmail);
    }

    [Fact]
    public async Task GetByPersonalEmailAsync_ShouldReturnNull_WhenNotExists()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var repository = new PendingEmployeeRegistrationRepository(
            database.Context);

        var result = await repository.GetByPersonalEmailAsync(
            "inexistente@email.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistRegistration()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var repository = new PendingEmployeeRegistrationRepository(
            database.Context);

        var registration = CreateRegistration();

        await repository.AddAsync(registration);
        await repository.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var result = await database.Context.PendingEmployeeRegistrations
            .FirstOrDefaultAsync(
                item => item.Id == registration.Id);

        Assert.NotNull(result);
        Assert.Equal(registration.FullName, result.FullName);
        Assert.Equal(registration.PersonalEmail, result.PersonalEmail);
        Assert.Equal(registration.PhoneNumber, result.PhoneNumber);
    }
}
