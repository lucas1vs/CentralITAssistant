
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Domain.Entities;

public class WhatsAppContactTests
{
    [Fact]
    public void Constructor_ShouldCreateContactWithValidData()
    {
        var contact = new WhatsAppContact("5561999999999");

        Assert.NotEqual(Guid.Empty, contact.Id);
        Assert.Equal("5561999999999", contact.PhoneNumber);
        Assert.False(contact.IsVerified);
        Assert.Null(contact.VerifiedAt);
        Assert.True(contact.IsActive);
        Assert.NotEqual(default(DateTime), contact.CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldTrimPhoneNumber()
    {
        var contact = new WhatsAppContact(" 5561999999999 ");

        Assert.Equal("5561999999999", contact.PhoneNumber);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenPhoneNumberIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new WhatsAppContact(""));
    }

    [Fact]
    public void Verify_ShouldSetContactAsVerified()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();

        Assert.True(contact.IsVerified);
        Assert.NotNull(contact.VerifiedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldResetVerification()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();
        contact.UpdatePhoneNumber("5561988888888");

        Assert.Equal("5561988888888", contact.PhoneNumber);
        Assert.False(contact.IsVerified);
        Assert.Null(contact.VerifiedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_WithSameNumber_ShouldKeepVerification()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();
        contact.UpdatePhoneNumber("5561999999999");

        Assert.True(contact.IsVerified);
        Assert.NotNull(contact.VerifiedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldTrimNewPhoneNumber()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.UpdatePhoneNumber(" 5561988888888 ");

        Assert.Equal("5561988888888", contact.PhoneNumber);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldThrowWhenPhoneNumberIsEmpty()
    {
        var contact = new WhatsAppContact("5561999999999");

        Assert.Throws<ArgumentException>(
            () => contact.UpdatePhoneNumber(""));
    }

    [Fact]
    public void Deactivate_ShouldSetContactAsInactive()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Deactivate();

        Assert.False(contact.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetContactAsActive()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Deactivate();
        contact.Activate();

        Assert.True(contact.IsActive);
    }

    [Fact]
    public void Activate_ShouldResetPreviousVerification()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();

        Assert.True(contact.IsVerified);
        Assert.NotNull(contact.VerifiedAt);

        contact.Deactivate();
        contact.Activate();

        Assert.True(contact.IsActive);
        Assert.False(contact.IsVerified);
        Assert.Null(contact.VerifiedAt);
    }

    [Fact]
    public void Activate_ShouldThrowWhenEmployeeAlreadyHasAnotherActiveContact()
    {
        var employee = new Employee(
            "Lucas Domingos",
            "lucas@email.com");

        var firstContact = new WhatsAppContact("5561999999999");
        var secondContact = new WhatsAppContact("5561988888888");

        employee.AssociateWhatsAppContact(firstContact);

        firstContact.Deactivate();

        employee.AssociateWhatsAppContact(secondContact);

        Assert.Throws<InvalidOperationException>(
            () => firstContact.Activate());

        Assert.False(firstContact.IsActive);
        Assert.True(secondContact.IsActive);
    }

    [Fact]
    public void Activate_ShouldThrowWhenEmployeeIsInactive()
    {
        var employee = new Employee(
            "Lucas Domingos",
            "lucas@email.com");

        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);
        employee.Deactivate();

        Assert.Throws<InvalidOperationException>(
            () => contact.Activate());
    }

    [Fact]
    public void Verify_ShouldThrowWhenContactIsInactive()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Deactivate();

        Assert.Throws<InvalidOperationException>(
            () => contact.Verify());
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldThrowWhenContactIsInactive()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Deactivate();

        Assert.Throws<InvalidOperationException>(
            () => contact.UpdatePhoneNumber("5561988888888"));
    }
}
