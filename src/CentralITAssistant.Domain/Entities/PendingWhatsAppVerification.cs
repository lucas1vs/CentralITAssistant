
namespace CentralITAssistant.Domain.Entities;

public class PendingWhatsAppVerification
{
    public const int MaxFailedAttempts = 5;
    public const int MaxResends = 5;
    public const int ResendIntervalSeconds = 60;

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime LastCodeSentAt { get; private set; }

    public int ResendCount { get; private set; }
    public int FailedAttempts { get; private set; }

    public bool IsVerified { get; private set; }
    public bool IsCancelled { get; private set; }

    private PendingWhatsAppVerification() { }

    public PendingWhatsAppVerification(
        Guid employeeId,
        string phoneNumber,
        string codeHash,
        DateTime expiresAt,
        DateTime sentAt)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("O funcionário é obrigatório.", nameof(employeeId));

        EmployeeId = employeeId;
        SetPhoneNumber(phoneNumber);
        SetCodeHash(codeHash);

        if (expiresAt <= sentAt)
            throw new ArgumentException("A expiração deve ser posterior ao envio.");

        Id = Guid.NewGuid();
        CreatedAt = sentAt;
        LastCodeSentAt = sentAt;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired(DateTime now)
        => now >= ExpiresAt;

    public bool CanAttemptVerification(DateTime now)
        => !IsVerified
           && !IsCancelled
           && !IsExpired(now)
           && FailedAttempts < MaxFailedAttempts;

    public bool CanResendCode(DateTime now)
        => !IsVerified
           && !IsCancelled
           && ResendCount < MaxResends
           && GetResendCooldown(now) == TimeSpan.Zero;

    public TimeSpan GetResendCooldown(DateTime now)
    {
        var nextAllowedAt = LastCodeSentAt.AddSeconds(ResendIntervalSeconds);
        var remaining = nextAllowedAt - now;

        return remaining > TimeSpan.Zero
            ? remaining
            : TimeSpan.Zero;
    }

    public void RenewVerification(
        string codeHash,
        DateTime expiresAt,
        DateTime sentAt)
    {
        if (IsVerified || IsCancelled)
            throw new InvalidOperationException(
                "Esta verificação não pode mais ser renovada.");

        if (ResendCount >= MaxResends)
            throw new InvalidOperationException(
                "Limite de reenvios atingido.");

        if (GetResendCooldown(sentAt) > TimeSpan.Zero)
            throw new InvalidOperationException(
                "Aguarde antes de solicitar outro código.");

        if (expiresAt <= sentAt)
            throw new ArgumentException(
                "A expiração deve ser posterior ao envio.");

        SetCodeHash(codeHash);

        ExpiresAt = expiresAt;
        LastCodeSentAt = sentAt;
        ResendCount++;
        FailedAttempts = 0;
    }

    public void RegisterFailedAttempt(DateTime now)
    {
        if (!CanAttemptVerification(now))
            throw new InvalidOperationException(
                "Não é possível registrar uma tentativa nesta verificação.");

        FailedAttempts++;
    }

    public void MarkAsVerified(DateTime now)
    {
        if (!CanAttemptVerification(now))
            throw new InvalidOperationException(
                "Esta verificação não pode ser concluída.");

        IsVerified = true;
    }

    public void Cancel()
    {
        if (IsVerified)
            throw new InvalidOperationException(
                "Uma verificação concluída não pode ser cancelada.");

        IsCancelled = true;
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException(
                "O número de WhatsApp é obrigatório.",
                nameof(phoneNumber));

        PhoneNumber = phoneNumber.Trim();
    }

    private void SetCodeHash(string codeHash)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException(
                "O hash do código é obrigatório.",
                nameof(codeHash));

        CodeHash = codeHash;
    }
}
