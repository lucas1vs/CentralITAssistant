
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Repositories;

namespace CentralITAssistant.Tests.Unit.Infrastructure.Repositories;

public class WhatsAppContactRepositoryTests
{
    [Fact]
    public async Task ExistsByPhoneNumberForAnotherEmployeeAsync_ShouldReturnTrue_WhenNumberBelongsToAnotherEmployee()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new WhatsAppContactRepository(database.Context);

        var owner = new Employee(
            "Ana Silva",
            "ana@gmail.com");

        var requestingEmployee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        var contact = new WhatsAppContact("5561999999999");

        owner.AssociateWhatsAppContact(contact);

        database.Context.Employees.AddRange(
            owner,
            requestingEmployee);

        await database.Context.SaveChangesAsync();

        var result =
            await repository.ExistsByPhoneNumberForAnotherEmployeeAsync(
                "5561999999999",
                requestingEmployee.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByPhoneNumberForAnotherEmployeeAsync_ShouldReturnFalse_WhenNumberBelongsToSameEmployee()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new WhatsAppContactRepository(database.Context);

        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com");

        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);

        database.Context.Employees.Add(employee);
        await database.Context.SaveChangesAsync();

        var result =
            await repository.ExistsByPhoneNumberForAnotherEmployeeAsync(
                "5561999999999",
                employee.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByPhoneNumberForAnotherEmployeeAsync_ShouldReturnFalse_WhenNumberDoesNotExist()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new WhatsAppContactRepository(database.Context);

        var result =
            await repository.ExistsByPhoneNumberForAnotherEmployeeAsync(
                "5561999999999",
                Guid.NewGuid());

        Assert.False(result);
    }
}
