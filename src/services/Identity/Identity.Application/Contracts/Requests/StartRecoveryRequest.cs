using GaniPay.Identity.Application.Contracts.Enums;

namespace GaniPay.Identity.Application.Contracts.Requests;

public sealed record class StartRecoveryRequest
{
    public string PhoneNumber { get; init; } = default!;
    public RecoveryChannel Channel { get; init; }
}
