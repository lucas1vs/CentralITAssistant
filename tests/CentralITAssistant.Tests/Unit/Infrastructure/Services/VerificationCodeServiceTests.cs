
using CentralITAssistant.Infrastructure.Services.Security;
using Xunit;

namespace CentralITAssistant.Tests.Unit.Infrastructure.Services;

public class VerificationCodeServiceTests
{
    private const string SecretKey =
        "CentralITAssistant-Test-Secret-Key-2026";

    private readonly VerificationCodeService _service;

    public VerificationCodeServiceTests()
    {
        _service = new VerificationCodeService(SecretKey);
    }

    [Fact]
    public void GenerateCode_ShouldReturnSixDigits()
    {
        // Act
        var code = _service.GenerateCode();

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.All(code, character =>
            Assert.True(char.IsAsciiDigit(character)));
    }

    [Fact]
    public void HashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        const string code = "123456";

        // Act
        var firstHash = _service.HashCode(code);
        var secondHash = _service.HashCode(code);

        // Assert
        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void HashCode_ShouldReturnDifferentHashesForDifferentCodes()
    {
        // Arrange
        const string firstCode = "123456";
        const string secondCode = "654321";

        // Act
        var firstHash = _service.HashCode(firstCode);
        var secondHash = _service.HashCode(secondCode);

        // Assert
        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void VerifyCode_ShouldReturnTrue_WhenCodeIsCorrect()
    {
        // Arrange
        const string code = "123456";
        var hash = _service.HashCode(code);

        // Act
        var result = _service.VerifyCode(code, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyCode_ShouldReturnFalse_WhenCodeIsIncorrect()
    {
        // Arrange
        var hash = _service.HashCode("123456");

        // Act
        var result = _service.VerifyCode("654321", hash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("12345a")]
    [InlineData("12 456")]
    public void VerifyCode_ShouldReturnFalse_WhenCodeIsInvalid(
        string code)
    {
        // Arrange
        var hash = _service.HashCode("123456");

        // Act
        var result = _service.VerifyCode(code, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyCode_ShouldReturnFalse_WhenHashIsInvalid()
    {
        // Act
        var result = _service.VerifyCode(
            "123456",
            "invalid-base64");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyCode_ShouldReturnFalse_WhenHashIsEmpty()
    {
        // Act
        var result = _service.VerifyCode("123456", "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashCode_ShouldThrow_WhenCodeIsInvalid()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _service.HashCode("12345"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenSecretKeyIsEmpty(
        string secretKey)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new VerificationCodeService(secretKey));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSecretKeyIsTooShort()
    {
        // Arrange
        const string shortKey = "short";

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new VerificationCodeService(shortKey));
    }

    [Fact]
    public void VerifyCode_ShouldReturnFalse_WhenHashBelongsToAnotherKey()
    {
        // Arrange
        const string code = "123456";
        var anotherService = new VerificationCodeService(
            "Another-Secret-Key-For-Testing-2026");

        var hash = anotherService.HashCode(code);

        // Act
        var result = _service.VerifyCode(code, hash);

        // Assert
        Assert.False(result);
    }
}
