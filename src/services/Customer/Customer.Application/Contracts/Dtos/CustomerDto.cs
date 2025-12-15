using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record class CustomerDto
{
    public Guid Id { get; init; }
    public string CustomerNumber { get; init; } = default!;

    public CustomerType Type { get; init; }
    public CustomerSegment Segment { get; init; }
    public CustomerStatus Status { get; init; }

    public DateOnly OpenDate { get; init; }
    public DateOnly? CloseDate { get; init; }
    public string? CloseReason { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IndividualCustomerDto? Individual { get; init; }

    public List<AddressDto> Addresses { get; init; } = [];
    public List<PhoneDto> Phones { get; init; } = [];
    public List<EmailDto> Emails { get; init; } = [];
}
