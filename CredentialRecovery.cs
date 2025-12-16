using GaniPay.Identity.Application.Contracts.Enums;

namespace GaniPay.Identity.Domain.Entities;

public sealed class CredentialRecovery
{
    public Guid Id { get; set; }
    public Guid CredentialId { get; set; }

    public RecoveryChannel Channel { get; set; }

    // internal only
    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
