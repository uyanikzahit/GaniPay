namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record class IndividualCustomerDto
{
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public DateOnly BirthDate { get; init; }
    public string Nationality { get; init; } = default!;
    public string IdentityNumber { get; init; } = default!;
}
