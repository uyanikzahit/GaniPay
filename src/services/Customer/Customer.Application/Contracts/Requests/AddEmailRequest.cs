using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Requests;

public sealed record AddEmailRequest(
    string EmailAddress,
    EmailType Type
);
