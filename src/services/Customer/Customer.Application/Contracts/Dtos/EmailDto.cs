using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record EmailDto(
    Guid Id,
    Guid CustomerId,
    string EmailAddress,
    EmailType Type,
    bool IsVerified
);
