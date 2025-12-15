using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts;

public sealed record class UpdateCustomerRequest
{
    public CustomerSegment? Segment { get; init; }
    public CustomerStatus? Status { get; init; }
    public DateOnly? CloseDate { get; init; }
    public string? CloseReason { get; init; }
}
