using GaniPay.Customer.Application.Contracts.Enums;
using GaniPay.Customer.Application.Contracts.Dtos;

namespace GaniPay.Customer.Application.Contracts;

public sealed record class CreateCustomerRequest
{
    public string? CustomerNumber { get; init; }
    public CustomerType Type { get; init; }
    public CustomerSegment Segment { get; init; }
    public DateOnly OpenDate { get; init; }

    public IndividualCustomerDto? Individual { get; init; }

    public List<AddressDto>? Addresses { get; init; }
    public List<PhoneDto>? Phones { get; init; }
    public List<EmailDto>? Emails { get; init; }
}
