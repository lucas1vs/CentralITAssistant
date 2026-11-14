
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Domain;

public class EmployeeTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateEmployee()
    {
        var employee = new Employee(
            "Lucas Domingos",
            "lucas@centralit.com.br",
            "lucas@gmail.com");

        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Equal("Lucas Domingos", employee.FullName);
        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
        Assert.True(employee.IsActive);
        Assert.Null(employee.WhatsAppContact);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("", "lucas@centralit.com.br"));
    }

    [Fact]
    public void Constructor_WithEmptyCorporateEmail_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("Lucas", ""));
    }

    [Fact]
    public void Constructor_WithUppercaseEmails_ShouldConvertEmailsToLowercase()
    {
        var employee = new Employee(
            "Lucas",
            "LUCAS@CENTRALIT.COM.BR",
            "LUCAS@GMAIL.COM");

        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
    }

    [Fact]
    public void Constructor_WithEmailsContainingSpaces_ShouldTrimEmails()
    {
        var employee = new Employee(
            "Lucas",
            " lucas@centralit.com.br ",
            " lucas@gmail.com ");

        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
    }

    [Fact]
    public void UpdatePersonalEmail_WithValidEmail_ShouldUpdatePersonalEmail()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        employee.UpdatePersonalEmail("novo@gmail.com");

        Assert.Equal("novo@gmail.com", employee.PersonalEmail);
    }

    [Fact]
    public void UpdatePersonalEmail_WithEmptyValue_ShouldSetPersonalEmailToNull()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br",
            "lucas@gmail.com");

        employee.UpdatePersonalEmail(" ");

        Assert.Null(employee.PersonalEmail);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        employee.Deactivate();

        Assert.False(employee.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        employee.Deactivate();
        employee.Activate();

        Assert.True(employee.IsActive);
    }

    [Fact]
    public void AssociateWhatsAppContact_WithValidContact_ShouldEstablishRelationship()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);

        Assert.Equal(contact, employee.WhatsAppContact);
        Assert.Equal(employee, contact.Employee);
        Assert.Equal(employee.Id, contact.EmployeeId);
    }

    [Fact]
    public void AssociateWhatsAppContact_WhenEmployeeAlreadyHasContact_ShouldThrowInvalidOperationException()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        var firstContact = new WhatsAppContact("5561999999999");
        var secondContact = new WhatsAppContact("5561888888888");

        employee.AssociateWhatsAppContact(firstContact);

        Assert.Throws<InvalidOperationException>(() =>
            employee.AssociateWhatsAppContact(secondContact));
    }

    [Fact]
    public void AssociateWhatsAppContact_WhenContactBelongsToAnotherEmployee_ShouldThrowInvalidOperationException()
    {
        var firstEmployee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        var secondEmployee = new Employee(
            "João",
            "joao@centralit.com.br");

        var contact = new WhatsAppContact("5561999999999");

        firstEmployee.AssociateWhatsAppContact(contact);

        Assert.Throws<InvalidOperationException>(() =>
            secondEmployee.AssociateWhatsAppContact(contact));
    }

    [Fact]
    public void AssociateWhatsAppContact_WithNullContact_ShouldThrowArgumentNullException()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        Assert.Throws<ArgumentNullException>(() =>
            employee.AssociateWhatsAppContact(null!));
    }
}
