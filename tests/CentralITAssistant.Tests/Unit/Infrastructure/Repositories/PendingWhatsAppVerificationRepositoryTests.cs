using Microsoft.EntityFrameworkCore;
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Repositories;


namespace CentralITAssistant.Tests.Unit.Infrastructure.Repositories;

public class PendingWhatsAppVerificationRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnVerification()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository =
            new PendingWhatsAppVerificationRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var verification = CreateVerification(employee.Id);

        database.Context.PendingWhatsAppVerifications.Add(verification);
        await database.Context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(verification.Id);

        Assert.NotNull(result);
        Assert.Equal(verification.Id, result.Id);
        Assert.Equal(employee.Id, result.EmployeeId);
        Assert.Equal(verification.PhoneNumber, result.PhoneNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository =
            new PendingWhatsAppVerificationRepository(database.Context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveByEmployeeIdAsync_ShouldReturnActiveVerification()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository =
            new PendingWhatsAppVerificationRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var activeVerification = CreateVerification(
            employee.Id,
            "5561999999999");

        var cancelledVerification = CreateVerification(
            employee.Id,
            "5561988888888");

        cancelledVerification.Cancel();

        var verifiedVerification = CreateVerification(
            employee.Id,
            "5561977777777");

        verifiedVerification.MarkAsVerified(DateTime.UtcNow);

        database.Context.PendingWhatsAppVerifications.AddRange(
            activeVerification,
            cancelledVerification,
            verifiedVerification);

        await database.Context.SaveChangesAsync();

        var result =
            await repository.GetActiveByEmployeeIdAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal(activeVerification.Id, result.Id);
        Assert.False(result.IsVerified);
        Assert.False(result.IsCancelled);
    }

    [Fact]
    public async Task GetActiveByEmployeeIdAsync_ShouldReturnNull_WhenNoActiveVerificationExists()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository =
            new PendingWhatsAppVerificationRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var verification = CreateVerification(employee.Id);

        verification.Cancel();

        database.Context.PendingWhatsAppVerifications.Add(verification);
        await database.Context.SaveChangesAsync();

        var result =
            await repository.GetActiveByEmployeeIdAsync(employee.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddVerification()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository =
            new PendingWhatsAppVerificationRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var verification = CreateVerification(employee.Id);

        await repository.AddAsync(verification);

        var resultBeforeSave = await database.Context
            .PendingWhatsAppVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == verification.Id);

        Assert.Null(resultBeforeSave);

        await repository.SaveChangesAsync();

        var resultAfterSave = await database.Context
            .PendingWhatsAppVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == verification.Id);

        Assert.NotNull(resultAfterSave);
        Assert.Equal(verification.Id, resultAfterSave.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistVerificationChanges()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository =
            new PendingWhatsAppVerificationRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var verification = CreateVerification(employee.Id);

        database.Context.PendingWhatsAppVerifications.Add(verification);
        await database.Context.SaveChangesAsync();

        verification.Cancel();

        await repository.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(verification.Id);

        Assert.NotNull(result);
        Assert.True(result.IsCancelled);
    }

    private static PendingWhatsAppVerification CreateVerification(
        Guid employeeId,
        string phoneNumber = "5561999999999")
    {
        var sentAt = DateTime.UtcNow;

        return new PendingWhatsAppVerification(
            employeeId,
            phoneNumber,
            "hashed-verification-code",
            sentAt.AddMinutes(5),
            sentAt);
    }
}
