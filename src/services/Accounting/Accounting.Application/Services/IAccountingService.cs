using GaniPay.Accounting.Application.Contracts.Dtos;
using GaniPay.Accounting.Application.Contracts.Requests;

namespace GaniPay.Accounting.Application.Services;

public interface IAccountingService
{
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, CancellationToken ct);
    Task<BalanceDto> GetBalanceAsync(Guid customerId, string currency, CancellationToken ct);

    Task<AccountingTransactionDto> PostTransactionAsync(PostAccountingTransactionRequest request, CancellationToken ct);

    Task<UsageResultDto> GetUsageAsync(UsageQueryRequest request, CancellationToken ct);
    Task<AccountStatusDto> GetAccountStatusAsync(Guid customerId, string currency, CancellationToken ct);

    Task<CustomerWalletsDto> GetCustomerWalletsAsync(Guid customerId, CancellationToken ct);


    Task<IReadOnlyList<AccountBalanceHistoryDto>> GetBalanceHistoryAsync(
        Guid accountId,
        CancellationToken ct);

}
