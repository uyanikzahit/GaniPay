using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Application.Contracts.Dtos;
using GaniPay.Expense.Application.Requests;
using GaniPay.Expense.Domain.Entities;
using GaniPay.Expense.Domain.Enums;

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
        if (entity is null) throw new InvalidOperationException("Expense bulunamadý.");
        return MapExpense(entity);
    }

    public async Task<ExpenseDto> GetByCodeAsync(string code, CancellationToken ct)
    {
        code = NormalizeCode(code);
        var entity = await _expenseRepo.GetByCodeAsync(code, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadý.");
        return MapExpense(entity);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Code zorunludur.");
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Name zorunludur.");

        var code = NormalizeCode(request.Code);

        var exists = await _expenseRepo.GetByCodeAsync(code, ct);
        if (exists is not null) throw new InvalidOperationException("Ayný code ile expense zaten var.");

        ValidateRule(request.MinAmount, request.MaxAmount, request.Percent, request.FixedAmount);

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
            Currency = NormalizeCurrency(request.Currency),
            IsVisible = request.IsVisible,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _expenseRepo.AddAsync(entity, ct);
        return MapExpense(entity);
    }

    public async Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct)
    {
        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadý.");

        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Name zorunludur.");
        ValidateRule(request.MinAmount, request.MaxAmount, request.Percent, request.FixedAmount);

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.MinAmount = request.MinAmount;
        entity.MaxAmount = request.MaxAmount;
        entity.Percent = request.Percent;
        entity.FixedAmount = request.FixedAmount;
        entity.Currency = NormalizeCurrency(request.Currency);
        entity.IsVisible = request.IsVisible;
        entity.UpdatedAt = DateTime.UtcNow;

        await _expenseRepo.UpdateAsync(entity, ct);
        return MapExpense(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new InvalidOperationException("Expense bulunamadý.");

        await _expenseRepo.DeleteAsync(entity, ct);
    }

    public async Task<ExpensePendingDto> CreatePendingAsync(CreateExpensePendingRequest request, CancellationToken ct)
    {
        if (request.AccountingTxId == Guid.Empty) throw new InvalidOperationException("AccountingTxId zorunludur.");
        if (request.ExpenseId == Guid.Empty) throw new InvalidOperationException("ExpenseId zorunludur.");
        if (request.BaseAmount <= 0) throw new InvalidOperationException("BaseAmount 0'dan büyük olmalýdýr.");

        var expense = await _expenseRepo.GetByIdAsync(request.ExpenseId, ct);
        if (expense is null) throw new InvalidOperationException("ExpenseDefinition bulunamadý.");

        // Min/Max kontrolü (varsa)
        if (expense.MinAmount.HasValue && request.BaseAmount < expense.MinAmount.Value)
            throw new InvalidOperationException("BaseAmount min_amount altýndadýr.");
        if (expense.MaxAmount.HasValue && request.BaseAmount > expense.MaxAmount.Value)
            throw new InvalidOperationException("BaseAmount max_amount üstündedir.");

        // Hesaplama: fixed varsa fixed, yoksa percent * base
        decimal calculated;
        if (expense.FixedAmount.HasValue && expense.FixedAmount.Value > 0)
            calculated = expense.FixedAmount.Value;
        else if (expense.Percent.HasValue && expense.Percent.Value > 0)
            calculated = Math.Round(request.BaseAmount * expense.Percent.Value, 2, MidpointRounding.AwayFromZero);
        else
            calculated = 0m;

        var now = DateTime.UtcNow;

        var pending = new ExpensePending
        {
            Id = Guid.NewGuid(),
            AccountingTxId = request.AccountingTxId,
            ExpenseId = expense.Id,
            CalculatedAmount = calculated,
            Currency = NormalizeCurrency(request.Currency),
            PendingStatus = ExpensePendingStatus.Pending,
            TransactionDate = request.TransactionDate ?? now,
            TryCount = 0,
            ResultCode = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _pendingRepo.AddAsync(pending, ct);
        return MapPending(pending);
    }

    private static void ValidateRule(decimal? min, decimal? max, decimal? percent, decimal? fixedAmount)
    {
        if (min.HasValue && min.Value < 0) throw new InvalidOperationException("MinAmount negatif olamaz.");
        if (max.HasValue && max.Value < 0) throw new InvalidOperationException("MaxAmount negatif olamaz.");
        if (min.HasValue && max.HasValue && min.Value > max.Value) throw new InvalidOperationException("MinAmount, MaxAmount'tan büyük olamaz.");

        if (percent.HasValue && percent.Value < 0) throw new InvalidOperationException("Percent negatif olamaz.");
        if (percent.HasValue && percent.Value > 1) throw new InvalidOperationException("Percent 1'den büyük olamaz. (0.02 gibi beklenir)");

        if (fixedAmount.HasValue && fixedAmount.Value < 0) throw new InvalidOperationException("FixedAmount negatif olamaz.");

        // ikisi birden doluysa da sorun yok (öncelik fixed), ama istersen strict yapabilirsin.
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    private static string NormalizeCurrency(string currency) => string.IsNullOrWhiteSpace(currency) ? "TRY" : currency.Trim().ToUpperInvariant();

    private static ExpenseDto MapExpense(ExpenseDefinition e) => new()
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

    private static ExpensePendingDto MapPending(ExpensePending p) => new()
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
