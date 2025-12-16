namespace GaniPay.Identity.Application.Contracts.Requests;

public sealed record class CompleteRegistrationRequest
{
    public Guid CredentialId { get; init; }

    // Step-2'de email alýnýr
    public string Email { get; init; } = default!;
}
