namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record CustomerWalletsDto(
    Guid CustomerId,
    List<AccountDto> Accounts
);