namespace GaniPay.Login.Worker.Models;

public sealed class AccountingWalletsResponse
{
    public string? CustomerId { get; set; }
    public List<AccountItem> Accounts { get; set; } = new();
}

public sealed class AccountItem
{
    public string? Id { get; set; }
    public string? CustomerId { get; set; }
    public string? Currency { get; set; }
    public decimal Balance { get; set; }
    public int Status { get; set; }
    public string? Iban { get; set; }
    public string? CreatedAt { get; set; }
}