namespace GaniPay.Accounting.Application.Contracts.Requests;

public sealed class CreateAccountRequest
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = "TRY";
}
