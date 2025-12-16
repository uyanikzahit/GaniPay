using GaniPay.Identity.Application.Contracts.Enums;

namespace GaniPay.Identity.Application.Contracts.Dtos;

public sealed record class CredentialRecoveryDto
{
    public Guid Id { get; init; }
    public Guid CredentialId { get; init; }
    public RecoveryChannel Channel { get; init; }

    public DateTime ExpiresAt { get; init; }
    public DateTime? UsedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
