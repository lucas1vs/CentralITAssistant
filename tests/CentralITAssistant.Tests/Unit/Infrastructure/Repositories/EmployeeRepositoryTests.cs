
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Repositories;

namespace CentralITAssistant.Tests.Unit.Infrastructure.Repositories;

public class EmployeeRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddEmployee()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        await repository.AddAsync(employee);

        var result = await database.Context.Employees.FindAsync(
            employee.Id);

        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenEmployeeIsNull()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.AddAsync(null!));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPersonalEmailAsync_ShouldReturnEmployee()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var result = await repository.GetByPersonalEmailAsync(
            "LUCAS@GMAIL.COM");

        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
    }

    [Fact]
    public async Task GetByPersonalEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var result = await repository.GetByPersonalEmailAsync(
            "inexistente@gmail.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPersonalEmailAsync_ShouldThrow_WhenEmailIsEmpty()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => repository.GetByPersonalEmailAsync(" "));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmployeesOrderedByName()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var employee1 = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        var employee2 = new Employee(
            "Ana Silva",
            "ana@gmail.com");

        var employee3 = new Employee(
            "Carlos Souza",
            "carlos@gmail.com");

        database.Context.Employees.AddRange(
            employee1,
            employee2,
            employee3);

        await database.Context.SaveChangesAsync();

        var result = await repository.GetAllAsync();

        Assert.Equal(3, result.Count);

        Assert.Equal(
            new[] { "Ana Silva", "Carlos Souza", "Lucas Domingos" },
            result.Select(e => e.FullName).ToArray());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new EmployeeRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        await repository.AddAsync(employee);
        await repository.SaveChangesAsync();

        var result = await database.Context.Employees
            .FindAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal(employee.PersonalEmail, result.PersonalEmail);
    }
}
