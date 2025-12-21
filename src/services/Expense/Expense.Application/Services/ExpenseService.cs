using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Application.Contracts.Dtos;
using GaniPay.Expense.Application.Requests;
using GaniPay.Expense.Domain.Entities;
using DomainPendingStatus = GaniPay.Expense.Domain.Enums.ExpensePendingStatus;

namespace GaniPay.Expense.Application.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly IExpensePendingRepository _pendingRepo;

    public ExpenseService(IExpenseRepository expenseRepo, IExpensePendingRepository pendingRepo)
    {
        _expenseRepo = expenseRepo;
        _pendingRepo = pendingRepo;
    }

    public async Task<List<ExpenseDto>> ListAsync(CancellationToken ct)
    {
        var list = await _expenseRepo.ListAsync(ct);
        return list.Select(MapExpense).ToList();
    }

    public async Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadı.");
        return MapExpense(entity);
    }

    public async Task<ExpenseDto> GetByCodeAsync(string code, CancellationToken ct)
    {
        code = NormalizeCode(code);
        var entity = await _expenseRepo.GetByCodeAsync(code, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadı.");
        return MapExpense(entity);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new InvalidOperationException("Code zorunludur.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Name zorunludur.");

        var code = NormalizeCode(request.Code);
        var currency = NormalizeCurrency(request.Currency);

        ValidateRule(request.MinAmount, request.MaxAmount, request.Percent, request.FixedAmount, currency);

        var exists = await _expenseRepo.GetByCodeAsync(code, ct);
        if (exists is not null)
            throw new InvalidOperationException("Aynı code ile expense zaten var.");

        var entity = new ExpenseDefinition
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            Percent = request.Percent,
            FixedAmount = request.FixedAmount,
            Currency = currency,
            IsVisible = request.IsVisible,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _expenseRepo.AddAsync(entity, ct);
        return MapExpense(entity);
    }

    public async Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct)
    {
        // DB’den çek -> tracked entity
        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadı.");

        var currency = NormalizeCurrency(request.Currency);

        ValidateRule(request.MinAmount, request.MaxAmount, request.Percent, request.FixedAmount, currency);

        // Tracked entity üzerinde alan güncelle (code değiştirmiyoruz)
        entity.Name = string.IsNullOrWhiteSpace(request.Name) ? entity.Name : request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.MinAmount = request.MinAmount;
        entity.MaxAmount = request.MaxAmount;
        entity.Percent = request.Percent;
        entity.FixedAmount = request.FixedAmount;
        entity.Currency = currency;
        entity.IsVisible = request.IsVisible;
        entity.UpdatedAt = DateTime.UtcNow;

        await _expenseRepo.UpdateAsync(entity, ct);

        return MapExpense(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadı.");

        await _expenseRepo.DeleteAsync(entity, ct);
    }

    public async Task<ExpensePendingDto> CreatePendingAsync(CreateExpensePendingRequest request, CancellationToken ct)
    {
        // Bu endpoint "fee calculation result" kaydı içindir.
        // Yani hesaplamayı burada yapmak zorunda değiliz; dış servis (payments/orchestrator) hesaplayıp gönderir.
        if (request.AccountingTxId == Guid.Empty)
            throw new InvalidOperationException("AccountingTxId zorunludur.");

        if (request.ExpenseId == Guid.Empty)
            throw new InvalidOperationException("ExpenseId zorunludur.");

        if (request.CalculatedAmount < 0)
            throw new InvalidOperationException("CalculatedAmount negatif olamaz.");

        // Expense var mı kontrol edelim
        var expense = await _expenseRepo.GetByIdAsync(request.ExpenseId, ct);
        if (expense is null)
            throw new InvalidOperationException("Expense tanımı bulunamadı.");

        // Currency boşsa expense.currency kullan
        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? expense.Currency
            : NormalizeCurrency(request.Currency);

        var pending = new ExpensePending
        {
            Id = Guid.NewGuid(),
            AccountingTxId = request.AccountingTxId,
            ExpenseId = request.ExpenseId,
            CalculatedAmount = request.CalculatedAmount,
            Currency = currency,
            PendingStatus = DomainPendingStatus.Pending,
            TransactionDate = DateTime.UtcNow,
            TryCount = 0,
            ResultCode = null
        };

        await _pendingRepo.AddAsync(pending, ct);
        return MapPending(pending);
    }

    // --------------------
    // Helpers
    // --------------------

    private static string NormalizeCode(string code)
        => (code ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeCurrency(string currency)
    {
        currency = (currency ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException("Currency zorunludur.");

        if (currency.Length != 3)
            throw new InvalidOperationException("Currency 3 karakter olmalıdır (TRY, USD vb.).");

        return currency;
    }

    private static void ValidateRule(decimal? minAmount, decimal? maxAmount, decimal? percent, decimal? fixedAmount, string currency)
    {
        if (minAmount.HasValue && minAmount.Value < 0) throw new InvalidOperationException("min_amount negatif olamaz.");
        if (maxAmount.HasValue && maxAmount.Value < 0) throw new InvalidOperationException("max_amount negatif olamaz.");
        if (minAmount.HasValue && maxAmount.HasValue && minAmount.Value > maxAmount.Value)
            throw new InvalidOperationException("min_amount, max_amount'tan büyük olamaz.");

        if (percent.HasValue && percent.Value < 0) throw new InvalidOperationException("percent negatif olamaz.");
        if (fixedAmount.HasValue && fixedAmount.Value < 0) throw new InvalidOperationException("fixed_amount negatif olamaz.");

        var p = percent.GetValueOrDefault(0);
        var f = fixedAmount.GetValueOrDefault(0);

        if (p <= 0 && f <= 0)
            throw new InvalidOperationException("percent veya fixed_amount en az birisi 0'dan büyük olmalıdır.");
    }

    private static ExpenseDto MapExpense(ExpenseDefinition e)
        => new ExpenseDto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            Description = e.Description,
            MinAmount = e.MinAmount,
            MaxAmount = e.MaxAmount,
            Percent = e.Percent,
            FixedAmount = e.FixedAmount,
            Currency = e.Currency,
            IsVisible = e.IsVisible
        };

    private static ExpensePendingDto MapPending(ExpensePending p)
        => new ExpensePendingDto
        {
            Id = p.Id,
            AccountingTxId = p.AccountingTxId,
            ExpenseId = p.ExpenseId,
            CalculatedAmount = p.CalculatedAmount,
            Currency = p.Currency,
            PendingStatus = p.PendingStatus.ToString(),
            TransactionDate = p.TransactionDate,
            TryCount = p.TryCount,
            ResultCode = p.ResultCode
        };
}
