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

        var currency = request.Currency.Trim();
        var accountNumber = request.AccountNumber.Trim();

        var existing = await _accountRepository.GetByCustomerAndCurrencyAsync(request.CustomerId, currency, ct);
        if (existing is not null)
            throw new InvalidOperationException("Account already exists for this customer & currency.");

        var createdAt = EnsureUtc(DateTime.UtcNow);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = 0m,
            Status = AccountStatus.Active,
            Iban = string.IsNullOrWhiteSpace(request.Iban) ? null : request.Iban.Trim(),
            CreatedAt = createdAt
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
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException("Currency is required.");

        var cur = currency.Trim();

        var account = await _accountRepository.GetByCustomerAndCurrencyAsync(customerId, cur, ct)
                      ?? throw new InvalidOperationException("Account not found.");

        return new BalanceDto(account.Id, account.CustomerId, account.Currency, account.Balance);
    }

    public async Task<AccountingTransactionDto> PostTransactionAsync(PostAccountingTransactionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidOperationException("Currency is required.");

        var currency = request.Currency.Trim();

        var account = await _accountRepository.GetByCustomerAndCurrencyAsync(request.CustomerId, currency, ct)
                      ?? throw new InvalidOperationException("Account not found.");

        if (account.Status != AccountStatus.Active)
            throw new InvalidOperationException("Account is not active.");

        if (request.Amount <= 0)
            throw new InvalidOperationException("Amount must be > 0.");

        // DB’den gelen datetime’lar bazen Unspecified gelebiliyor.
        // Update sýrasýnda EF bunu da "touched" ederse timestamptz insert/update patlayabiliyor.
        account.CreatedAt = EnsureUtc(account.CreatedAt);

        var directionString = ToDbDirection(request.Direction);

        var balanceBefore = account.Balance;

        var balanceDelta = request.Direction switch
        {
            Contracts.Enums.AccountingDirection.Credit => +request.Amount,
            Contracts.Enums.AccountingDirection.Debit => -request.Amount,
            _ => 0m
        };

        var balanceAfter = balanceBefore + balanceDelta;

        if (balanceAfter < 0)
            throw new InvalidOperationException("Insufficient balance.");

        var createdAt = EnsureUtc(DateTime.UtcNow);

        // accounting_transaction insert
        var tx = new AccountingTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Direction = directionString,
            Amount = request.Amount,
            Currency = currency,
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
            Currency = currency,
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

        var metric = Normalize(request.MetricType);

        var usedCount = txs.Count;
        var usedAmount = txs.Sum(x => x.Amount);

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

    private static DateTime EnsureUtc(DateTime dt)
    {
        // Npgsql timestamptz için Unspecified yazmayý sevmez.
        // Elimizdeki deðer Unspecified ise UTC varsayarak düzelt.
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

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

        // DateOnly -> DateTime (UTC)
        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return (fromUtc, toUtc, fromDate, toDate);
    }
}
