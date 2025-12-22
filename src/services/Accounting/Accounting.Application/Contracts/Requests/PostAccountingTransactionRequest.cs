using System.Text.Json.Serialization;
using GaniPay.Accounting.Application.Contracts.Enums;

namespace GaniPay.Accounting.Application.Contracts.Requests;

public sealed record PostAccountingTransactionRequest(
    Guid CustomerId,
    string Currency,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    AccountingDirection Direction,
    decimal Amount,
    AccountingOperationType OperationType,
    Guid ReferenceId
);
