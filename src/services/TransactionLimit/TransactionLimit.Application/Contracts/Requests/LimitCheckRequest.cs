namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed class LimitCheckRequest
{
    public Guid CustomerId { get; set; }
    public Guid LimitDefinitionId { get; set; }

    public short Year { get; set; }
    public short? Month { get; set; }
    public short? Day { get; set; }

    public decimal Value { get; set; }
    public string Currency { get; set; } = "TRY";
}
