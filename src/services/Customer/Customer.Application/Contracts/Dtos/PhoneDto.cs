namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record class PhoneDto
{
    public string CountryCode { get; init; } = default!;
    public string PhoneNumber { get; init; } = default!;
    public int Type { get; init; }
}
