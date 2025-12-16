using GaniPay.Identity.Application.Contracts.Enums;

namespace GaniPay.Identity.Domain.Entities;

public sealed class Credential
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = "";

    // internal only
    public string PasswordHash { get; set; } = default!;

    public int FailedLoginCount { get; set; }
    public CredentialStatus Status { get; set; }

    public DateTime? LockoutEndAt { get; set; }
    public string? LockReason { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    public RegistrationStatus RegistrationStatus { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
