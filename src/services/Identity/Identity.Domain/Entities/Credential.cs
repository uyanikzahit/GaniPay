namespace GaniPay.Identity.Domain.Entities;

public sealed class Credential
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    // login
    public string LoginType { get; set; } = default!;   // "phone" / "email" / "username"
    public string LoginValue { get; set; } = default!;  // normalized

    // password
    public string PasswordHash { get; set; } = default!;
    public string PasswordSalt { get; set; } = default!;
    public string PasswordAlgo { get; set; } = default!;

    // lock / audit
    public int FailedLoginCount { get; set; }
    public bool IsLocked { get; set; }
    public string? LockReason { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // navigation
    public List<CredentialRecovery> Recoveries { get; set; } = new();
}
