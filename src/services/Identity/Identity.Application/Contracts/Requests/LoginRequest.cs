namespace GaniPay.Identity.Application.Contracts.Requests;

public sealed record class LoginRequest
{
    public string PhoneNumber { get; init; } = default!;
    public string Password { get; init; } = default!;
}
