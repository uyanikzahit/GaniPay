using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Requests;

public sealed record CreateIndividualCustomerRequest(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Nationality,
    string IdentityNumber,
    CustomerSegment Segment
);
