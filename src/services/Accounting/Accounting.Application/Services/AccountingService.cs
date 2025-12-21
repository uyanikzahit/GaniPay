using GaniPay.Accounting.Application.Abstractions;
using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Application.Contracts.Dtos;
using GaniPay.Accounting.Application.Contracts.Enums;
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
        if (request.CustomerId == Guid.Empty)
            throw new InvalidOperationException("customerId is required.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidOperationException("currency is required.");

        var currency = request.Currency.Trim().ToUpperInvariant();

        if (await _accountRepository.ExistsAsync(request.CustomerId, currency, ct))
            throw new InvalidOperationException("Account already exists for this customer and currency.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Currency = currency,
            Balance = 0m,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);

        return MapAccountDto(account);
    }

    public async Task<BalanceDto> GetBalanceAsync(Guid customerId, string currency, CancellationToken ct)
    {
        if (customerId == Guid.Empty)
            throw new InvalidOperationException("customerId is required.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException("currency is required.");

        var cur = currency.Trim().ToUpperInvariant();

        var account = await _accountRepository.GetByCustomerAndCurrencyAsync(customerId, cur, ct);
        if (account is null)
            throw new InvalidOperationException("Account not found.");

        if (account.Status != AccountStatus.Active)
            throw new InvalidOperationException("Account is not active.");

        return new BalanceDto
        {
            CustomerId = account.CustomerId,
            Currency = account.Currency,
            Balance = account.Balance,
            AsOfUtc = DateTime.UtcNow
        };
    }

    public async Task<AccountingTransactionDto> BookTransactionAsync(BookTransactionRequest request, CancellationToken ct)
    {
        ValidateBookRequest(request);

        var currency = request.Currency.Trim().ToUpperInvariant();

        var account = await _accountRepository.GetByCustomerAndCurrencyAsync(request.CustomerId, currency, ct);
        if (account is null)
            throw new InvalidOperationException("Account not found.");

        if (account.Status != AccountStatus.Active)
            throw new InvalidOperationException("Account is not active.");

        // idempotency check
        var existing = await _txRepository.GetByIdempotencyKeyAsync(account.Id, request.IdempotencyKey.Trim(), ct);
        if (existing is not null)
            return MapTxDto(existing);

        var domainOp = MapToDomainOperation(request.OperationType);
        var (direction, signedDelta) = GetEntryEffect(domainOp, request.Amount);

        var balanceBefore = account.Balance;
        var balanceAfter = balanceBefore + signedDelta;

        if (balanceAfter < 0m)
            throw new InvalidOperationException("Insufficient funds.");

        var now = DateTime.UtcNow;

        var tx = new AccountingTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Direction = direction,
            OperationType = domainOp,
            Amount = request.Amount,
            Currency = account.Currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,

            ReferenceId = request.ReferenceId.Trim(),
            IdempotencyKey = request.IdempotencyKey.Trim(),
            CorrelationId = request.CorrelationId.Trim(),

            Status = TransactionStatus.Booked,
            CreatedAt = now,
            BookedAt = now
        };

        await _txRepository.AddAsync(tx, ct);

        var history = new AccountBalanceHistory
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Direction = direction,
            ChangeAmount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Currency = account.Currency,
            OperationType = domainOp,

            // ✅ Guid -> string hatasını bitiren net çözüm
            ReferenceId = tx.Id.ToString(),

            CreatedAt = now
        };

        await _historyRepository.AddAsync(history, ct);

        account.Balance = balanceAfter;
        await _accountRepository.UpdateAsync(account, ct);

        await _uow.SaveChangesAsync(ct);

        return MapTxDto(tx);
    }

    public async Task<UsageResultDto> GetUsageAsync(UsageQueryRequest request, CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
            throw new InvalidOperationException("customerId is required.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidOperationException("currency is required.");

        var currency = request.Currency.Trim().ToUpperInvariant();

        var (fromUtc, toUtc) = BuildRange(DateOnly.FromDateTime(DateTime.UtcNow), request.Period);

        var metricString = request.MetricType.ToString();

        var value = await _txRepository.CalculateUsageAsync(
            request.CustomerId,
            currency,
            metricString,
            fromUtc,
            toUtc,
            ct);

        return new UsageResultDto
        {
            CustomerId = request.CustomerId,
            Currency = currency,
            MetricType = request.MetricType,
            Period = request.Period,
            Value = value,
            FromUtc = fromUtc,
            ToUtc = toUtc
        };
    }

    private static void ValidateBookRequest(BookTransactionRequest request)
    {
        if (request.CustomerId == Guid.Empty) throw new InvalidOperationException("customerId is required.");
        if (string.IsNullOrWhiteSpace(request.Currency)) throw new InvalidOperationException("currency is required.");
        if (request.Amount <= 0m) throw new InvalidOperationException("amount must be > 0.");
        if (string.IsNullOrWhiteSpace(request.ReferenceId)) throw new InvalidOperationException("referenceId is required.");
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new InvalidOperationException("idempotencyKey is required.");
        if (string.IsNullOrWhiteSpace(request.CorrelationId)) throw new InvalidOperationException("correlationId is required.");
    }

    private static (EntryDirection direction, decimal signedDelta) GetEntryEffect(OperationType opType, decimal amount)
        => opType switch
        {
            OperationType.TopUp => (EntryDirection.Credit, +amount),
            OperationType.TransferIn => (EntryDirection.Credit, +amount),
            OperationType.TransferOut => (EntryDirection.Debit, -amount),
            OperationType.Fee => (EntryDirection.Debit, -amount),
            _ => (EntryDirection.Credit, +amount)
        };

    private static OperationType MapToDomainOperation(AccountingOperationType op) => op switch
    {
        AccountingOperationType.TopUp => OperationType.TopUp,
        AccountingOperationType.TransferIn => OperationType.TransferIn,
        AccountingOperationType.TransferOut => OperationType.TransferOut,
        AccountingOperationType.Fee => OperationType.Fee,
        _ => OperationType.TopUp
    };

    private static AccountingOperationType MapToContractOperation(OperationType op) => op switch
    {
        OperationType.TopUp => AccountingOperationType.TopUp,
        OperationType.TransferIn => AccountingOperationType.TransferIn,
        OperationType.TransferOut => AccountingOperationType.TransferOut,
        OperationType.Fee => AccountingOperationType.Fee,
        _ => AccountingOperationType.TopUp
    };

    private static AccountingEntryDirection MapToContractDirection(EntryDirection d) => d switch
    {
        EntryDirection.Credit => AccountingEntryDirection.Credit,
        EntryDirection.Debit => AccountingEntryDirection.Debit,
        _ => AccountingEntryDirection.Credit
    };

    private static AccountDto MapAccountDto(Account a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        Currency = a.Currency,
        Balance = a.Balance,
        Status = a.Status,
        CreatedAt = a.CreatedAt
    };

    private static AccountingTransactionDto MapTxDto(AccountingTransaction tx) => new()
    {
        Id = tx.Id,
        AccountId = tx.AccountId,
        Direction = MapToContractDirection(tx.Direction),
        OperationType = MapToContractOperation(tx.OperationType),
        Amount = tx.Amount,
        Currency = tx.Currency,
        BalanceBefore = tx.BalanceBefore,
        BalanceAfter = tx.BalanceAfter,
        ReferenceId = tx.ReferenceId,
        IdempotencyKey = tx.IdempotencyKey,
        CorrelationId = tx.CorrelationId,
        Status = tx.Status,
        CreatedAt = tx.CreatedAt,
        BookedAt = tx.BookedAt
    };

    private static (DateTime fromUtc, DateTime toUtc) BuildRange(DateOnly date, UsagePeriod period)
    {
        DateOnly fromDate;
        DateOnly toDate;

        switch (period)
        {
            case UsagePeriod.Day:
                fromDate = date;
                toDate = date;
                break;

            case UsagePeriod.Month:
                fromDate = new DateOnly(date.Year, date.Month, 1);
                toDate = fromDate.AddMonths(1).AddDays(-1);
                break;

            case UsagePeriod.Year:
            default:
                fromDate = new DateOnly(date.Year, 1, 1);
                toDate = new DateOnly(date.Year, 12, 31);
                break;
        }

        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return (fromUtc, toUtc);
    }
}
