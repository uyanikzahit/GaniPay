namespace GaniPay.Identity.Application.Contracts.Requests;

public sealed record class CompleteRecoveryRequest
{
    public string Token { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
}
