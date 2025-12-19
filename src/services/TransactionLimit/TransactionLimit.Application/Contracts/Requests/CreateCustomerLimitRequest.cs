using GaniPay.TransactionLimit.Application.Contracts.Enums;

namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed class CreateCustomerLimitRequest
{
    // body’den de gelebilir, path ile kontrol ediyoruz (boş bırakılabilir)
    public Guid CustomerId { get; set; }

    public Guid LimitDefinitionId { get; set; }

    public short Year { get; set; }
    public short? Month { get; set; }
    public short? Day { get; set; }

    public decimal Value { get; set; }
    public string Currency { get; set; } = "TRY";

    public LimitSource Source { get; set; } = LimitSource.Admin;
    public string? Reason { get; set; }
    public string? UpdatedBy { get; set; }
}
