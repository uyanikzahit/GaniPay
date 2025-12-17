using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Requests;

public sealed record AddAddressRequest(
    AddressType AddressType,
    string City,
    string District,
    string PostalCode,
    string AddressLine1
);
