using GaniPay.Identity.Domain.Enums;

namespace GaniPay.Identity.Domain.Entities;

public sealed class CredentialRecovery
{
    public Guid Id { get; set; }

    public Guid CredentialId { get; set; }
    public Credential Credential { get; set; } = default!;

    public RecoveryChannel Channel { get; set; }

    // DB'de token değil token_hash saklanır
    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
