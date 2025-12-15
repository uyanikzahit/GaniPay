namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record class AddressDto
{
    public string AddressType { get; init; } = default!;
    public string City { get; init; } = default!;
    public string District { get; init; } = default!;
    public string PostalCode { get; init; } = default!;
    public string AddressLine { get; init; } = default!;
}
