namespace CentralITAssistant.Domain.Entities;

public class PendingEmployeeRegistration
{
    public const int MaxFailedAttempts = 5;
    public const int MaxResends = 5;
    public static readonly TimeSpan ResendInterval = TimeSpan.FromSeconds(60);

    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string PersonalEmail { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string CodeHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime LastCodeSentAt { get; private set; }

    public int ResendCount { get; private set; }

    public int FailedAttempts { get; private set; }

    public bool IsVerified { get; private set; }

    private PendingEmployeeRegistration()
    {
    }

    public PendingEmployeeRegistration(
        string fullName,
        string personalEmail,
        string phoneNumber,
        string codeHash,
        DateTime expiresAt)
    {
        SetFullName(fullName);
        SetPersonalEmail(personalEmail);
        SetPhoneNumber(phoneNumber);
        SetCodeHash(codeHash);

        var now = DateTime.UtcNow;

        if (expiresAt <= now)
            throw new ArgumentException(
                "A data de expiração deve ser futura.");

        Id = Guid.NewGuid();
        CreatedAt = now;
        LastCodeSentAt = now;
        ExpiresAt = expiresAt;
        ResendCount = 0;
        FailedAttempts = 0;
        IsVerified = false;
    }

    public bool IsExpired(DateTime utcNow)
    {
        return utcNow >= ExpiresAt;
    }

    public bool CanAttemptVerification(DateTime utcNow)
    {
        return !IsVerified
            && !IsExpired(utcNow)
            && FailedAttempts < MaxFailedAttempts;
    }

    public bool CanResendCode(DateTime utcNow)
    {
        return !IsVerified
            && ResendCount < MaxResends
            && GetResendCooldown(utcNow) == TimeSpan.Zero;
    }

    public TimeSpan GetResendCooldown(DateTime utcNow)
    {
        var remaining = LastCodeSentAt + ResendInterval - utcNow;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public void RenewVerification(
        string codeHash,
        DateTime utcNow,
        DateTime expiresAt)
    {
        if (IsVerified)
            throw new InvalidOperationException(
                "Esta verificação já foi concluída.");

        if (ResendCount >= MaxResends)
            throw new InvalidOperationException(
                "O limite de reenvios de código foi atingido.");

        if (GetResendCooldown(utcNow) > TimeSpan.Zero)
            throw new InvalidOperationException(
                "Aguarde antes de solicitar um novo código.");

        if (expiresAt <= utcNow)
            throw new ArgumentException(
                "A data de expiração deve ser futura.");

        SetCodeHash(codeHash);

        ExpiresAt = expiresAt;
        LastCodeSentAt = utcNow;
        FailedAttempts = 0;
        ResendCount++;
    }

    public void RegisterFailedAttempt()
    {
        if (IsVerified)
            throw new InvalidOperationException(
                "Esta verificação já foi concluída.");

        if (FailedAttempts >= MaxFailedAttempts)
            throw new InvalidOperationException(
                "O limite de tentativas foi atingido.");

        FailedAttempts++;
    }

    public void MarkAsVerified(DateTime utcNow)
    {
        if (IsVerified)
            throw new InvalidOperationException(
                "Esta verificação já foi concluída.");

        if (IsExpired(utcNow))
            throw new InvalidOperationException(
                "O código de verificação expirou.");

        if (FailedAttempts >= MaxFailedAttempts)
            throw new InvalidOperationException(
                "O limite de tentativas foi atingido.");

        IsVerified = true;
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "O nome é obrigatório.");

        FullName = fullName.Trim();
    }

    private void SetPersonalEmail(string personalEmail)
    {
        if (string.IsNullOrWhiteSpace(personalEmail))
            throw new ArgumentException(
                "O e-mail pessoal é obrigatório.");

        PersonalEmail = personalEmail.Trim().ToLowerInvariant();
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException(
                "O número de WhatsApp é obrigatório.");

        PhoneNumber = phoneNumber.Trim();
    }

    private void SetCodeHash(string codeHash)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException(
                "O hash do código é obrigatório.");

        CodeHash = codeHash;
    }
}