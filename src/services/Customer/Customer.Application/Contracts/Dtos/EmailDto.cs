namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record class EmailDto
{
    public string Email { get; init; } = default!;
    public int Type { get; init; }
}
