using GaniPay.Identity.Application.Contracts.Enums;

namespace GaniPay.Identity.Application.Contracts.Dtos;

public sealed record class CredentialDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }

    public string PhoneNumber { get; init; } = default!;
    public string Email { get; init; } = default!;

    public int FailedLoginCount { get; init; }
    public CredentialStatus Status { get; init; }

    public DateTime? LockoutEndAt { get; init; }
    public string? LockReason { get; init; }

    public DateTime? LastLoginAt { get; init; }
    public DateTime? PhoneVerifiedAt { get; init; }
    public DateTime? EmailVerifiedAt { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
