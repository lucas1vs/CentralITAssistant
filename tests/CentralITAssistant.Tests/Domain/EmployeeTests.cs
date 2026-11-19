
using CentralITAssistant.Domain.Entities;
using Xunit;

namespace CentralITAssistant.Tests.Unit.Domain;

public class EmployeeTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateEmployee()
    {
        var employee = new Employee(
            "Lucas Domingos",
            "lucas@gmail.com",
            "lucas@centralit.com.br");

        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Equal("Lucas Domingos", employee.FullName);
        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
        Assert.True(employee.IsActive);
        Assert.Empty(employee.WhatsAppContacts);
    }

    [Fact]
    public void Constructor_WithoutCorporateEmail_ShouldCreateEmployee()
    {
        var employee = new Employee("Lucas Domingos", "lucas@gmail.com");

        Assert.Equal("Lucas Domingos", employee.FullName);
        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
        Assert.Null(employee.CorporateEmail);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("", "lucas@gmail.com"));
    }

    [Fact]
    public void Constructor_WithEmptyPersonalEmail_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("Lucas", ""));
    }

    [Fact]
    public void Constructor_WithUppercaseEmails_ShouldConvertEmailsToLowercase()
    {
        var employee = new Employee(
            "Lucas",
            "LUCAS@GMAIL.COM",
            "LUCAS@CENTRALIT.COM.BR");

        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
    }

    [Fact]
    public void Constructor_WithEmailsContainingSpaces_ShouldTrimEmails()
    {
        var employee = new Employee(
            "Lucas",
            " lucas@gmail.com ",
            " lucas@centralit.com.br ");

        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
    }

    [Fact]
    public void UpdatePersonalEmail_WithValidEmail_ShouldUpdatePersonalEmail()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        employee.UpdatePersonalEmail("novo@gmail.com");

        Assert.Equal("novo@gmail.com", employee.PersonalEmail);
    }

    [Fact]
    public void UpdatePersonalEmail_WithEmptyValue_ShouldThrowArgumentException()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        Assert.Throws<ArgumentException>(() =>
            employee.UpdatePersonalEmail(" "));

        Assert.Equal("lucas@gmail.com", employee.PersonalEmail);
    }

    [Fact]
    public void UpdateCorporateEmail_WithValidEmail_ShouldUpdateCorporateEmail()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        employee.UpdateCorporateEmail("lucas@centralit.com.br");

        Assert.Equal("lucas@centralit.com.br", employee.CorporateEmail);
    }

    [Fact]
    public void UpdateCorporateEmail_WithEmptyValue_ShouldSetCorporateEmailToNull()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@gmail.com",
            "lucas@centralit.com.br");

        employee.UpdateCorporateEmail(" ");

        Assert.Null(employee.CorporateEmail);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        employee.Deactivate();

        Assert.False(employee.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        employee.Deactivate();
        employee.Activate();

        Assert.True(employee.IsActive);
    }

    [Fact]
    public void AssociateWhatsAppContact_WithValidContact_ShouldEstablishRelationship()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");
        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);

        Assert.Contains(contact, employee.WhatsAppContacts);
        Assert.Equal(employee, contact.Employee);
        Assert.Equal(employee.Id, contact.EmployeeId);
    }

    [Fact]
    public void AssociateWhatsAppContact_WhenEmployeeAlreadyHasContact_ShouldThrowInvalidOperationException()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        var firstContact = new WhatsAppContact("5561999999999");
        var secondContact = new WhatsAppContact("5561888888888");

        employee.AssociateWhatsAppContact(firstContact);

        Assert.Throws<InvalidOperationException>(() =>
            employee.AssociateWhatsAppContact(secondContact));
    }

    [Fact]
    public void AssociateWhatsAppContact_WhenContactBelongsToAnotherEmployee_ShouldThrowInvalidOperationException()
    {
        var firstEmployee = new Employee("Lucas", "lucas@gmail.com");
        var secondEmployee = new Employee("João", "joao@gmail.com");
        var contact = new WhatsAppContact("5561999999999");

        firstEmployee.AssociateWhatsAppContact(contact);

        Assert.Throws<InvalidOperationException>(() =>
            secondEmployee.AssociateWhatsAppContact(contact));
    }

    [Fact]
    public void AssociateWhatsAppContact_WithNullContact_ShouldThrowArgumentNullException()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        Assert.Throws<ArgumentNullException>(() =>
            employee.AssociateWhatsAppContact(null!));
    }

    [Fact]
    public void ReplaceWhatsAppContact_WithValidContact_ShouldReplaceAndPreserveOldContact()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        var oldContact = new WhatsAppContact("5561999999999");
        var newContact = new WhatsAppContact("5561888888888");

        employee.AssociateWhatsAppContact(oldContact);
        employee.ReplaceWhatsAppContact(newContact);

        Assert.Equal(2, employee.WhatsAppContacts.Count);
        Assert.Contains(oldContact, employee.WhatsAppContacts);
        Assert.Contains(newContact, employee.WhatsAppContacts);

        Assert.False(oldContact.IsActive);
        Assert.True(newContact.IsActive);

        Assert.Equal(employee.Id, oldContact.EmployeeId);
        Assert.Equal(employee.Id, newContact.EmployeeId);

        Assert.Same(employee, newContact.Employee);
        Assert.False(newContact.IsVerified);
    }

    [Fact]
    public void ReplaceWhatsAppContact_WhenEmployeeHasNoActiveContact_ShouldThrowInvalidOperationException()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");
        var newContact = new WhatsAppContact("5561888888888");

        Assert.Throws<InvalidOperationException>(() =>
            employee.ReplaceWhatsAppContact(newContact));

        Assert.Empty(employee.WhatsAppContacts);
        Assert.True(newContact.IsActive);
        Assert.Null(newContact.EmployeeId);
    }

    [Fact]
    public void ReplaceWhatsAppContact_WithNullContact_ShouldThrowArgumentNullException()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");
        var oldContact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(oldContact);

        Assert.Throws<ArgumentNullException>(() =>
            employee.ReplaceWhatsAppContact(null!));

        Assert.True(oldContact.IsActive);
    }

    [Fact]
    public void ReplaceWhatsAppContact_WhenNewContactBelongsToAnotherEmployee_ShouldKeepCurrentContactActive()
    {
        var firstEmployee = new Employee("Lucas", "lucas@gmail.com");
        var secondEmployee = new Employee("João", "joao@gmail.com");

        var currentContact = new WhatsAppContact("5561999999999");
        var contactFromAnotherEmployee = new WhatsAppContact("5561888888888");

        firstEmployee.AssociateWhatsAppContact(currentContact);
        secondEmployee.AssociateWhatsAppContact(contactFromAnotherEmployee);

        Assert.Throws<InvalidOperationException>(() =>
            firstEmployee.ReplaceWhatsAppContact(contactFromAnotherEmployee));

        Assert.True(currentContact.IsActive);
        Assert.True(contactFromAnotherEmployee.IsActive);

        Assert.Single(firstEmployee.WhatsAppContacts);
        Assert.Single(secondEmployee.WhatsAppContacts);
    }

    [Fact]
    public void Deactivate_ShouldDeactivateActiveWhatsAppContact()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");
        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);
        employee.Deactivate();

        Assert.False(employee.IsActive);
        Assert.False(contact.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldPreserveAllWhatsAppContactsAndDeactivateOnlyOnes()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");

        var oldContact = new WhatsAppContact("5561999999999");
        var currentContact = new WhatsAppContact("5561888888888");

        employee.AssociateWhatsAppContact(oldContact);
        employee.ReplaceWhatsAppContact(currentContact);

        employee.Deactivate();

        Assert.Equal(2, employee.WhatsAppContacts.Count);
        Assert.Contains(oldContact, employee.WhatsAppContacts);
        Assert.Contains(currentContact, employee.WhatsAppContacts);

        Assert.False(oldContact.IsActive);
        Assert.False(currentContact.IsActive);
    }

    [Fact]
    public void ActivateEmployee_ShouldNotReactivateWhatsAppContact()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");
        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);

        employee.Deactivate();
        employee.Activate();

        Assert.True(employee.IsActive);
        Assert.False(contact.IsActive);
    }

    [Fact]
    public void AssociateWhatsAppContact_WhenEmployeeIsInactive_ShouldThrowInvalidOperationException()
    {
        var employee = new Employee("Lucas", "lucas@gmail.com");
        var contact = new WhatsAppContact("5561999999999");

        employee.Deactivate();

        Assert.Throws<InvalidOperationException>(() =>
            employee.AssociateWhatsAppContact(contact));

        Assert.Empty(employee.WhatsAppContacts);
    }
}
