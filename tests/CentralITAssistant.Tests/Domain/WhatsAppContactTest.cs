
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Domain;

public class WhatsAppContactTests
{
    [Fact]
    public void Constructor_WithValidPhoneNumber_ShouldCreateUnassociatedContact()
    {
        var contact = new WhatsAppContact("5561999999999");

        Assert.NotEqual(Guid.Empty, contact.Id);
        Assert.Equal("5561999999999", contact.PhoneNumber);
        Assert.Null(contact.EmployeeId);
        Assert.Null(contact.Employee);
        Assert.False(contact.IsVerified);
        Assert.Null(contact.VerifiedAt);
        Assert.True(contact.IsActive);
    }

    [Fact]
    public void Constructor_WithPhoneNumberContainingSpaces_ShouldTrimPhoneNumber()
    {
        var contact = new WhatsAppContact(" 5561999999999 ");

        Assert.Equal("5561999999999", contact.PhoneNumber);
    }

    [Fact]
    public void Constructor_WithEmptyPhoneNumber_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new WhatsAppContact(""));
    }

    [Fact]
    public void Verify_ShouldSetIsVerifiedToTrueAndSetVerifiedAt()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();

        Assert.True(contact.IsVerified);
        Assert.NotNull(contact.VerifiedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_WithNewNumber_ShouldResetVerification()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();
        contact.UpdatePhoneNumber("5561888888888");

        Assert.Equal("5561888888888", contact.PhoneNumber);
        Assert.False(contact.IsVerified);
        Assert.Null(contact.VerifiedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_WithSameNumber_ShouldKeepVerification()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Verify();
        var verifiedAt = contact.VerifiedAt;

        contact.UpdatePhoneNumber("5561999999999");

        Assert.True(contact.IsVerified);
        Assert.Equal(verifiedAt, contact.VerifiedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_WithSpaces_ShouldTrimPhoneNumber()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.UpdatePhoneNumber(" 5561888888888 ");

        Assert.Equal("5561888888888", contact.PhoneNumber);
    }

    [Fact]
    public void UpdatePhoneNumber_WithEmptyNumber_ShouldThrowArgumentException()
    {
        var contact = new WhatsAppContact("5561999999999");

        Assert.Throws<ArgumentException>(() =>
            contact.UpdatePhoneNumber(""));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Deactivate();

        Assert.False(contact.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var contact = new WhatsAppContact("5561999999999");

        contact.Deactivate();
        contact.Activate();

        Assert.True(contact.IsActive);
    }
}
