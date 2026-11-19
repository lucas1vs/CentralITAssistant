
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Domain;

public class PendingEmployeeRegistrationTests
{
    private static PendingEmployeeRegistration CreateRegistration(
        DateTime? expiresAt = null)
    {
        return new PendingEmployeeRegistration(
            fullName: "Lucas Domingos",
            personalEmail: "LUCAS@EMAIL.COM",
            phoneNumber: "5561999999999",
            codeHash: "hashed-code",
            expiresAt: expiresAt ?? DateTime.UtcNow.AddMinutes(5));
    }

    [Fact]
    public void Constructor_ShouldCreateRegistrationWithNormalizedData()
    {
        // Act
        var registration = CreateRegistration();

        // Assert
        Assert.NotEqual(Guid.Empty, registration.Id);
        Assert.Equal("Lucas Domingos", registration.FullName);
        Assert.Equal("lucas@email.com", registration.PersonalEmail);
        Assert.Equal("5561999999999", registration.PhoneNumber);
        Assert.Equal("hashed-code", registration.CodeHash);
        Assert.Equal(0, registration.FailedAttempts);
        Assert.False(registration.IsVerified);
    }

    [Fact]
    public void Constructor_ShouldTrimFullNameAndPhoneNumber()
    {
        // Act
        var registration = new PendingEmployeeRegistration(
            "  Lucas Domingos  ",
            "  LUCAS@EMAIL.COM  ",
            "  5561999999999  ",
            "hashed-code",
            DateTime.UtcNow.AddMinutes(5));

        // Assert
        Assert.Equal("Lucas Domingos", registration.FullName);
        Assert.Equal("lucas@email.com", registration.PersonalEmail);
        Assert.Equal("5561999999999", registration.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowWhenFullNameIsInvalid(string? fullName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PendingEmployeeRegistration(
                fullName!,
                "lucas@email.com",
                "5561999999999",
                "hashed-code",
                DateTime.UtcNow.AddMinutes(5)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowWhenPersonalEmailIsInvalid(string? email)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PendingEmployeeRegistration(
                "Lucas Domingos",
                email!,
                "5561999999999",
                "hashed-code",
                DateTime.UtcNow.AddMinutes(5)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowWhenPhoneNumberIsInvalid(string? phoneNumber)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PendingEmployeeRegistration(
                "Lucas Domingos",
                "lucas@email.com",
                phoneNumber!,
                "hashed-code",
                DateTime.UtcNow.AddMinutes(5)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowWhenCodeHashIsInvalid(string? codeHash)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PendingEmployeeRegistration(
                "Lucas Domingos",
                "lucas@email.com",
                "5561999999999",
                codeHash!,
                DateTime.UtcNow.AddMinutes(5)));
    }

    [Fact]
    public void Constructor_ShouldThrowWhenExpirationIsInThePast()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            CreateRegistration(DateTime.UtcNow.AddMinutes(-1)));
    }

    [Fact]
    public void IsExpired_ShouldReturnFalseBeforeExpiration()
    {
        // Arrange
        var registration = CreateRegistration();

        // Act
        var result = registration.IsExpired(DateTime.UtcNow);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_ShouldReturnTrueAtExpirationTime()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddMinutes(5);
        var registration = CreateRegistration(expiration);

        // Act
        var result = registration.IsExpired(expiration);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAttemptVerification_ShouldReturnTrueWhenRegistrationIsValid()
    {
        // Arrange
        var registration = CreateRegistration();

        // Act
        var result = registration.CanAttemptVerification(DateTime.UtcNow);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAttemptVerification_ShouldReturnFalseWhenRegistrationIsExpired()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddMinutes(5);
        var registration = CreateRegistration(expiration);

        // Act
        var result = registration.CanAttemptVerification(expiration);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RegisterFailedAttempt_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var registration = CreateRegistration();

        // Act
        registration.RegisterFailedAttempt();

        // Assert
        Assert.Equal(1, registration.FailedAttempts);
    }

    [Fact]
    public void RegisterFailedAttempt_ShouldAllowUpToFiveAttempts()
    {
        // Arrange
        var registration = CreateRegistration();

        // Act
        for (var i = 0; i < 5; i++)
        {
            registration.RegisterFailedAttempt();
        }

        // Assert
        Assert.Equal(5, registration.FailedAttempts);
        Assert.False(registration.CanAttemptVerification(DateTime.UtcNow));
    }

    [Fact]
    public void RegisterFailedAttempt_ShouldThrowWhenLimitIsReached()
    {
        // Arrange
        var registration = CreateRegistration();

        for (var i = 0; i < 5; i++)
        {
            registration.RegisterFailedAttempt();
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            registration.RegisterFailedAttempt);
    }

    [Fact]
    public void MarkAsVerified_ShouldMarkRegistrationAsVerified()
    {
        // Arrange
        var registration = CreateRegistration();

        // Act
        registration.MarkAsVerified(DateTime.UtcNow);

        // Assert
        Assert.True(registration.IsVerified);
        Assert.False(registration.CanAttemptVerification(DateTime.UtcNow));
    }

    [Fact]
    public void MarkAsVerified_ShouldThrowWhenRegistrationIsExpired()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddMinutes(5);
        var registration = CreateRegistration(expiration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => registration.MarkAsVerified(expiration));
    }

    [Fact]
    public void MarkAsVerified_ShouldThrowWhenAttemptLimitIsReached()
    {
        // Arrange
        var registration = CreateRegistration();

        for (var i = 0; i < 5; i++)
        {
            registration.RegisterFailedAttempt();
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => registration.MarkAsVerified(DateTime.UtcNow));
    }

    [Fact]
    public void MarkAsVerified_ShouldThrowWhenAlreadyVerified()
    {
        // Arrange
        var registration = CreateRegistration();
        registration.MarkAsVerified(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => registration.MarkAsVerified(DateTime.UtcNow));
    }

    [Fact]
    public void RegisterFailedAttempt_ShouldThrowWhenAlreadyVerified()
    {
        // Arrange
        var registration = CreateRegistration();
        registration.MarkAsVerified(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            registration.RegisterFailedAttempt);
    }
}
