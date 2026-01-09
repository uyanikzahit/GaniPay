using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GaniPay.Transfer.Worker.Models;

public sealed class AccountingGetAccountResponse
{
    [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
    [JsonPropertyName("accounts")] public List<AccountingWalletAccountDto> Accounts { get; set; } = new();
}

public sealed class AccountingWalletAccountDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
    [JsonPropertyName("accountNumber")] public string? AccountNumber { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("status")] public int Status { get; set; }
    [JsonPropertyName("iban")] public string? Iban { get; set; }
    [JsonPropertyName("createdAt")] public DateTimeOffset? CreatedAt { get; set; }
}
