using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Requests;

public sealed record AddPhoneRequest(
    string CountryCode,
    string PhoneNumber,
    PhoneType Type
);
