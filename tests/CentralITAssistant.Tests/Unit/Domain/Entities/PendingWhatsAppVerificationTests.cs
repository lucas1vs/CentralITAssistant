
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Domain.Entities;

public class PendingWhatsAppVerificationTests
{
    private static readonly DateTime SentAt =
        new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

    private static PendingWhatsAppVerification CreateValidVerification()
    {
        return new PendingWhatsAppVerification(
            Guid.NewGuid(),
            "+5561999999999",
            "hash-do-codigo",
            SentAt.AddMinutes(5),
            SentAt);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateVerification()
    {
        var employeeId = Guid.NewGuid();

        var verification = new PendingWhatsAppVerification(
            employeeId,
            "+5561999999999",
            "hash-do-codigo",
            SentAt.AddMinutes(5),
            SentAt);

        Assert.NotEqual(Guid.Empty, verification.Id);
        Assert.Equal(employeeId, verification.EmployeeId);
        Assert.Equal("+5561999999999", verification.PhoneNumber);
        Assert.Equal("hash-do-codigo", verification.CodeHash);
        Assert.Equal(SentAt, verification.CreatedAt);
        Assert.Equal(SentAt, verification.LastCodeSentAt);
        Assert.Equal(SentAt.AddMinutes(5), verification.ExpiresAt);
        Assert.Equal(0, verification.ResendCount);
        Assert.Equal(0, verification.FailedAttempts);
        Assert.False(verification.IsVerified);
        Assert.False(verification.IsCancelled);
    }

    [Fact]
    public void Constructor_WithEmptyEmployeeId_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PendingWhatsAppVerification(
                Guid.Empty,
                "+5561999999999",
                "hash-do-codigo",
                SentAt.AddMinutes(5),
                SentAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidPhoneNumber_ShouldThrowException(
        string? phoneNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            new PendingWhatsAppVerification(
                Guid.NewGuid(),
                phoneNumber!,
                "hash-do-codigo",
                SentAt.AddMinutes(5),
                SentAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidCodeHash_ShouldThrowException(
        string? codeHash)
    {
        Assert.Throws<ArgumentException>(() =>
            new PendingWhatsAppVerification(
                Guid.NewGuid(),
                "+5561999999999",
                codeHash!,
                SentAt.AddMinutes(5),
                SentAt));
    }

    [Fact]
    public void Constructor_WithExpirationBeforeOrEqualSentAt_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PendingWhatsAppVerification(
                Guid.NewGuid(),
                "+5561999999999",
                "hash-do-codigo",
                SentAt,
                SentAt));
    }

    [Fact]
    public void IsExpired_WhenBeforeExpiration_ShouldReturnFalse()
    {
        var verification = CreateValidVerification();

        Assert.False(verification.IsExpired(SentAt.AddMinutes(4)));
    }

    [Fact]
    public void IsExpired_WhenAtExpiration_ShouldReturnTrue()
    {
        var verification = CreateValidVerification();

        Assert.True(verification.IsExpired(SentAt.AddMinutes(5)));
    }

    [Fact]
    public void CanAttemptVerification_WhenValid_ShouldReturnTrue()
    {
        var verification = CreateValidVerification();

        Assert.True(
            verification.CanAttemptVerification(SentAt.AddMinutes(1)));
    }

    [Fact]
    public void CanAttemptVerification_WhenExpired_ShouldReturnFalse()
    {
        var verification = CreateValidVerification();

        Assert.False(
            verification.CanAttemptVerification(SentAt.AddMinutes(5)));
    }

    [Fact]
    public void RegisterFailedAttempt_ShouldIncrementFailedAttempts()
    {
        var verification = CreateValidVerification();

        verification.RegisterFailedAttempt(SentAt.AddMinutes(1));

        Assert.Equal(1, verification.FailedAttempts);
    }

    [Fact]
    public void RegisterFailedAttempt_WhenLimitReached_ShouldBlockFurtherAttempts()
    {
        var verification = CreateValidVerification();

        for (var i = 0; i < PendingWhatsAppVerification.MaxFailedAttempts; i++)
        {
            verification.RegisterFailedAttempt(SentAt.AddMinutes(1));
        }

        Assert.Equal(5, verification.FailedAttempts);

        Assert.False(
            verification.CanAttemptVerification(SentAt.AddMinutes(1)));

        Assert.Throws<InvalidOperationException>(() =>
            verification.RegisterFailedAttempt(SentAt.AddMinutes(1)));
    }

    [Fact]
    public void GetResendCooldown_Before60Seconds_ShouldReturnRemainingTime()
    {
        var verification = CreateValidVerification();

        var result = verification.GetResendCooldown(
            SentAt.AddSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), result);
    }

    [Fact]
    public void GetResendCooldown_After60Seconds_ShouldReturnZero()
    {
        var verification = CreateValidVerification();

        var result = verification.GetResendCooldown(
            SentAt.AddSeconds(60));

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void CanResendCode_BeforeCooldown_ShouldReturnFalse()
    {
        var verification = CreateValidVerification();

        Assert.False(
            verification.CanResendCode(SentAt.AddSeconds(30)));
    }

    [Fact]
    public void CanResendCode_AfterCooldown_ShouldReturnTrue()
    {
        var verification = CreateValidVerification();

        Assert.True(
            verification.CanResendCode(SentAt.AddSeconds(60)));
    }

    [Fact]
    public void RenewVerification_ShouldUpdateCodeAndResetFailedAttempts()
    {
        var verification = CreateValidVerification();

        verification.RegisterFailedAttempt(SentAt.AddMinutes(1));
        verification.RegisterFailedAttempt(SentAt.AddMinutes(1));

        var newSentAt = SentAt.AddSeconds(60);

        verification.RenewVerification(
            "novo-hash",
            newSentAt.AddMinutes(5),
            newSentAt);

        Assert.Equal("+5561999999999", verification.PhoneNumber);
        Assert.Equal("novo-hash", verification.CodeHash);
        Assert.Equal(newSentAt, verification.LastCodeSentAt);
        Assert.Equal(newSentAt.AddMinutes(5), verification.ExpiresAt);
        Assert.Equal(1, verification.ResendCount);
        Assert.Equal(0, verification.FailedAttempts);
    }

    [Fact]
    public void RenewVerification_ShouldKeepPhoneNumberUnchanged()
    {
        var verification = CreateValidVerification();
        var originalPhoneNumber = verification.PhoneNumber;
        var newSentAt = SentAt.AddSeconds(60);

        verification.RenewVerification(
            "novo-hash",
            newSentAt.AddMinutes(5),
            newSentAt);

        Assert.Equal(originalPhoneNumber, verification.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void RenewVerification_WithInvalidCodeHash_ShouldThrowException(
        string? codeHash)
    {
        var verification = CreateValidVerification();
        var newSentAt = SentAt.AddSeconds(60);

        Assert.Throws<ArgumentException>(() =>
            verification.RenewVerification(
                codeHash!,
                newSentAt.AddMinutes(5),
                newSentAt));
    }

    [Fact]
    public void RenewVerification_WhenCooldownNotElapsed_ShouldThrowException()
    {
        var verification = CreateValidVerification();

        Assert.Throws<InvalidOperationException>(() =>
            verification.RenewVerification(
                "novo-hash",
                SentAt.AddMinutes(5),
                SentAt.AddSeconds(30)));
    }

    [Fact]
    public void RenewVerification_WhenMaximumResendsReached_ShouldThrowException()
    {
        var verification = CreateValidVerification();

        var currentSentAt = SentAt;

        for (var i = 0; i < PendingWhatsAppVerification.MaxResends; i++)
        {
            currentSentAt = currentSentAt.AddSeconds(60);

            verification.RenewVerification(
                $"hash-{i}",
                currentSentAt.AddMinutes(5),
                currentSentAt);
        }

        Assert.Equal(5, verification.ResendCount);

        currentSentAt = currentSentAt.AddSeconds(60);

        Assert.Throws<InvalidOperationException>(() =>
            verification.RenewVerification(
                "outro-hash",
                currentSentAt.AddMinutes(5),
                currentSentAt));
    }

    [Fact]
    public void MarkAsVerified_WhenValid_ShouldMarkVerificationAsCompleted()
    {
        var verification = CreateValidVerification();

        verification.MarkAsVerified(SentAt.AddMinutes(1));

        Assert.True(verification.IsVerified);
        Assert.False(
            verification.CanAttemptVerification(SentAt.AddMinutes(1)));
        Assert.False(
            verification.CanResendCode(SentAt.AddMinutes(1)));
    }

    [Fact]
    public void MarkAsVerified_WhenExpired_ShouldThrowException()
    {
        var verification = CreateValidVerification();

        Assert.Throws<InvalidOperationException>(() =>
            verification.MarkAsVerified(SentAt.AddMinutes(5)));
    }

    [Fact]
    public void Cancel_ShouldMarkVerificationAsCancelled()
    {
        var verification = CreateValidVerification();

        verification.Cancel();

        Assert.True(verification.IsCancelled);
        Assert.False(
            verification.CanAttemptVerification(SentAt.AddMinutes(1)));
        Assert.False(
            verification.CanResendCode(SentAt.AddMinutes(1)));
    }

    [Fact]
    public void Cancel_WhenAlreadyVerified_ShouldThrowException()
    {
        var verification = CreateValidVerification();

        verification.MarkAsVerified(SentAt.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() =>
            verification.Cancel());
    }
}
