using System.Text.Json.Serialization;
using GaniPay.Accounting.Application.Contracts.Enums;

namespace GaniPay.Accounting.Application.Contracts.Requests;

/// <summary>
/// Payments / Workflow adýmý Accounting'e "book" gibi düþünebilirsin.
/// DB'ye göre: accounting_transaction + account_balance_history + account.balance update.
/// </summary>
public sealed record PostAccountingTransactionRequest(
    Guid CustomerId,
    string Currency,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    AccountingDirection Direction,
    decimal Amount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    AccountingOperationType OperationType,
    Guid ReferenceId
);
