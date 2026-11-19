
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CentralITAssistant.Application.Abstractions.Services;

namespace CentralITAssistant.Infrastructure.Services.Security;

public class VerificationCodeService : IVerificationCodeService
{
    private readonly byte[] _secretKey;

    public VerificationCodeService(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException(
                "A chave secreta é obrigatória.",
                nameof(secretKey));

        _secretKey = Encoding.UTF8.GetBytes(secretKey);

        if (_secretKey.Length < 32)
            throw new ArgumentException(
                "A chave secreta deve possuir pelo menos 32 bytes.",
                nameof(secretKey));
    }

    public string GenerateCode()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);

        return number.ToString("D6", CultureInfo.InvariantCulture);
    }

    public string HashCode(string code)
    {
        if (!IsValidCode(code))
            throw new ArgumentException(
                "O código deve possuir exatamente 6 dígitos.",
                nameof(code));

        var payload = Encoding.UTF8.GetBytes(
            $"CentralITAssistant:EmailVerification:{code}");

        var hash = HMACSHA256.HashData(_secretKey, payload);

        return Convert.ToBase64String(hash);
    }

    public bool VerifyCode(string code, string codeHash)
    {
        if (!IsValidCode(code) || string.IsNullOrWhiteSpace(codeHash))
            return false;

        try
        {
            var expectedHash = Convert.FromBase64String(codeHash);

            var payload = Encoding.UTF8.GetBytes(
                $"CentralITAssistant:EmailVerification:{code}");

            var actualHash = HMACSHA256.HashData(_secretKey, payload);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsValidCode(string code)
    {
        return !string.IsNullOrEmpty(code)
            && code.Length == 6
            && code.All(char.IsAsciiDigit);
    }
}
