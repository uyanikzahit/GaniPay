using GaniPay.Accounting.Application.Abstractions;
using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Application.Contracts.Dtos;
using GaniPay.Accounting.Application.Contracts.Requests;
using GaniPay.Accounting.Domain.Entities;
using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Application.Services;

public sealed class AccountingService : IAccountingService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountingTransactionRepository _txRepository;
    private readonly IAccountBalanceHistoryRepository _historyRepository;
    private readonly IUnitOfWork _uow;

    public AccountingService(
        IAccountRepository accountRepository,
        IAccountingTransactionRepository txRepository,
        IAccountBalanceHistoryRepository historyRepository,
        IUnitOfWork uow)
    {
        _accountRepository = accountRepository;
        _txRepository = txRepository;
        _historyRepository = historyRepository;
        _uow = uow;
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidOperationException("Currency is required.");

        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            throw new InvalidOperationException("AccountNumber is required.");

        var existing = await _accountRepository.GetByCustomerAndCurrencyAsync(request.CustomerId, request.Currency, ct);
        if (existing is not null)
            throw new InvalidOperationException("Account already exists for this customer & currency.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            AccountNumber = request.AccountNumber.Trim(),
            Currency = request.Currency.Trim(),
            Balance = 0m,
            Status = AccountStatus.Active,
            Iban = string.IsNullOrWhiteSpace(request.Iban) ? null : request.Iban.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);

        return new AccountDto(
            account.Id,
            account.CustomerId,
            account.AccountNumber,
            account.Currency,
            account.Balance,
            (int)account.Status,
            account.Iban,
            account.CreatedAt
        );
    }

    public async Task<BalanceDto> GetBalanceAsync(Guid customerId, string currency, CancellationToken ct)
    {
        var account = await _accountRepository.GetByCustomerAndCurrencyAsync(customerId, currency, ct)
                      ?? throw new InvalidOperationException("Account not found.");

        return new BalanceDto(account.Id, account.CustomerId, account.Currency, account.Balance);
    }

    public async Task<AccountingTransactionDto> PostTransactionAsync(PostAccountingTransactionRequest request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByCustomerAndCurrencyAsync(request.CustomerId, request.Currency, ct)
                      ?? throw new InvalidOperationException("Account not found.");

        if (account.Status != AccountStatus.Active)
            throw new InvalidOperationException("Account is not active.");

        if (request.Amount <= 0)
            throw new InvalidOperationException("Amount must be > 0.");

        var directionString = ToDbDirection(request.Direction);
        var balanceBefore = account.Balance;

        var balanceDelta = request.Direction switch
        {
            Contracts.Enums.AccountingDirection.Credit => +request.Amount,
            Contracts.Enums.AccountingDirection.Debit => -request.Amount,
            _ => 0m
        };

        var balanceAfter = balanceBefore + balanceDelta;

        // MVP: Negatif bakiye istemiyorsan burada kes:
        // (Ýstersen bunu Payments/Camunda akýþýnda da kontrol edebiliriz ama burada garanti daha iyi.)
        if (balanceAfter < 0)
            throw new InvalidOperationException("Insufficient balance.");

        var createdAt = DateTime.UtcNow;

        // accounting_transaction insert
        var tx = new AccountingTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Direction = directionString,
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            OperationType = (int)request.OperationType,
            ReferenceId = request.ReferenceId,
            CreatedAt = createdAt
        };

        await _txRepository.AddAsync(tx, ct);

        // account_balance_history insert
        var history = new AccountBalanceHistory
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Direction = directionString,
            ChangeAmount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Currency = request.Currency,
            OperationType = (int)request.OperationType,
            ReferenceId = request.ReferenceId,
            CreatedAt = createdAt
        };

        await _historyRepository.AddAsync(history, ct);

        // account balance update
        account.Balance = balanceAfter;
        await _accountRepository.UpdateAsync(account, ct);

        await _uow.SaveChangesAsync(ct);

        return new AccountingTransactionDto(
            tx.Id,
            tx.AccountId,
            tx.Direction,
            tx.Amount,
            tx.Currency,
            tx.BalanceBefore,
            tx.BalanceAfter,
            tx.OperationType,
            tx.ReferenceId,
            tx.CreatedAt
        );
    }

    public async Task<UsageResultDto> GetUsageAsync(UsageQueryRequest request, CancellationToken ct)
    {
        var (fromUtc, toUtc, fromDate, toDate) = BuildRange(request.Date, request.Period);

        var txs = await _txRepository.ListByCustomerAndCurrencyAndRangeAsync(
            request.CustomerId,
            request.Currency,
            fromUtc,
            toUtc,
            ct);

        // MetricType: amount/count
        var metric = Normalize(request.MetricType);

        var usedCount = txs.Count;
        var usedAmount = txs.Sum(x => x.Amount);

        // Eðer "count" istiyorsa usedAmount yine de dönüyoruz (debug/monitoring için)
        // Eðer "amount" istiyorsa usedCount yine de dönüyoruz
        _ = metric;

        return new UsageResultDto(
            request.CustomerId,
            request.Currency,
            Normalize(request.Period),
            metric,
            fromDate,
            toDate,
            usedAmount,
            usedCount
        );
    }

    private static string ToDbDirection(Contracts.Enums.AccountingDirection dir)
        => dir == Contracts.Enums.AccountingDirection.Debit ? "debit" : "credit";

    private static string Normalize(string s)
        => (s ?? string.Empty).Trim().ToLowerInvariant();

    private static (DateTime fromUtc, DateTime toUtc, DateOnly fromDate, DateOnly toDate) BuildRange(DateOnly date, string period)
    {
        var p = Normalize(period);

        DateOnly fromDate;
        DateOnly toDate;

        if (p == "day")
        {
            fromDate = date;
            toDate = date;
        }
        else if (p == "month")
        {
            fromDate = new DateOnly(date.Year, date.Month, 1);
            toDate = fromDate.AddMonths(1).AddDays(-1);
        }
        else // year (default)
        {
            fromDate = new DateOnly(date.Year, 1, 1);
            toDate = new DateOnly(date.Year, 12, 31);
        }

        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return (fromUtc, toUtc, fromDate, toDate);
    }
}
