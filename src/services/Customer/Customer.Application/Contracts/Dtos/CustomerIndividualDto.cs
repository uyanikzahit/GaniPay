namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record CustomerIndividualDto(
    Guid CustomerId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Nationality,
    string IdentityNumber
);
