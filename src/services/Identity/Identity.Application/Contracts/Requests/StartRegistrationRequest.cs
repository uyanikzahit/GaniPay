namespace GaniPay.Identity.Application.Contracts.Requests;

public sealed record class StartRegistrationRequest
{
    public Guid CustomerId { get; init; }

    // login sadece telefon
    public string PhoneNumber { get; init; } = default!;

    // Step-1'de password alýnýr
    public string Password { get; init; } = default!;
}
