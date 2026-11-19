
using CentralITAssistant.Domain.Entities;
using CentralITAssistant.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentralITAssistant.Tests.Unit.Infrastructure;

public class AppDbContextModelTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=CentralITAssistantTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void Model_ShouldContainAllConfiguredEntities()
    {
        using var context = CreateContext();

        var employee = context.Model.FindEntityType(typeof(Employee));
        var contact = context.Model.FindEntityType(typeof(WhatsAppContact));
        var conversation = context.Model.FindEntityType(typeof(Conversation));

        Assert.NotNull(employee);
        Assert.NotNull(contact);
        Assert.NotNull(conversation);

        Assert.Equal("Employees", employee.GetTableName());
        Assert.Equal("WhatsAppContacts", contact.GetTableName());
        Assert.Equal("Conversations", conversation.GetTableName());
    }

    [Fact]
    public void Employee_ShouldHaveUniquePersonalEmail()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Employee))!;

        var index = entityType.GetIndexes()
            .Single(i => i.Properties.Any(
                p => p.Name == nameof(Employee.PersonalEmail)));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Employee_ShouldHaveUniqueFilteredCorporateEmail()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Employee))!;

        var index = entityType.GetIndexes()
            .Single(i => i.Properties.Any(
                p => p.Name == nameof(Employee.CorporateEmail)));

        Assert.True(index.IsUnique);
        Assert.Equal("[CorporateEmail] IS NOT NULL", index.GetFilter());
    }

    [Fact]
    public void Employee_ShouldHaveRequiredNameAndPersonalEmail()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Employee))!;

        var fullName = entityType.FindProperty(nameof(Employee.FullName))!;
        var personalEmail = entityType.FindProperty(
            nameof(Employee.PersonalEmail))!;

        Assert.False(fullName.IsNullable);
        Assert.Equal(150, fullName.GetMaxLength());

        Assert.False(personalEmail.IsNullable);
        Assert.Equal(256, personalEmail.GetMaxLength());
    }

    [Fact]
    public void Employee_CorporateEmail_ShouldBeOptional()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Employee))!;

        var corporateEmail = entityType.FindProperty(
            nameof(Employee.CorporateEmail))!;

        Assert.True(corporateEmail.IsNullable);
        Assert.Equal(256, corporateEmail.GetMaxLength());
    }

    [Fact]
    public void WhatsAppContact_ShouldHaveUniquePhoneNumber()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(
            typeof(WhatsAppContact))!;

        var phoneNumber = entityType.FindProperty(
            nameof(WhatsAppContact.PhoneNumber))!;

        var index = entityType.GetIndexes()
            .Single(i => i.Properties.Any(
                p => p.Name == nameof(WhatsAppContact.PhoneNumber)));

        Assert.False(phoneNumber.IsNullable);
        Assert.Equal(20, phoneNumber.GetMaxLength());
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void WhatsAppContact_ShouldHaveManyToOneRelationshipWithEmployee()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(
            typeof(WhatsAppContact))!;

        var foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.Properties.Any(
                p => p.Name == nameof(WhatsAppContact.EmployeeId)));

        Assert.False(foreignKey.IsUnique);
        Assert.False(foreignKey.IsRequired);

        Assert.Equal(
            typeof(Employee),
            foreignKey.PrincipalEntityType.ClrType);

        Assert.Equal(
            nameof(Employee.WhatsAppContacts),
            foreignKey.PrincipalToDependent?.Name);

        Assert.Equal(
            nameof(WhatsAppContact.Employee),
            foreignKey.DependentToPrincipal?.Name);
    }

    [Fact]
    public void WhatsAppContact_ShouldHaveUniqueFilteredIndexForActiveContacts()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(
            typeof(WhatsAppContact))!;

        var index = entityType.GetIndexes()
            .Single(i =>
                i.IsUnique &&
                i.Properties.Any(
                    p => p.Name == nameof(WhatsAppContact.EmployeeId)));

        Assert.True(index.IsUnique);

        Assert.Equal(
            "[EmployeeId] IS NOT NULL AND [IsActive] = 1",
            index.GetFilter());
    }

    [Fact]
    public void Conversation_ShouldHaveOptionalRelationshipWithEmployee()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(
            typeof(Conversation))!;

        var foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.Properties.Any(
                p => p.Name == nameof(Conversation.EmployeeId)));

        Assert.False(foreignKey.IsRequired);
        Assert.Equal(typeof(Employee), foreignKey.PrincipalEntityType.ClrType);
    }

    [Fact]
    public void Conversation_ShouldHaveRequiredRelationshipWithWhatsAppContact()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(
            typeof(Conversation))!;

        var foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.Properties.Any(
                p => p.Name == nameof(Conversation.WhatsAppContactId)));

        Assert.True(foreignKey.IsRequired);
        Assert.Equal(
            typeof(WhatsAppContact),
            foreignKey.PrincipalEntityType.ClrType);
    }

    [Fact]
    public void Conversation_ShouldHaveUniqueFilteredIndexForActiveEmployeeConversations()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(
            typeof(Conversation))!;

        var index = entityType.GetIndexes()
            .Single(i => i.Properties.Any(
                p => p.Name == nameof(Conversation.EmployeeId)));

        Assert.True(index.IsUnique);

        Assert.Equal(
            "[EmployeeId] IS NOT NULL AND [IsActive] = 1",
            index.GetFilter());
    }
}
