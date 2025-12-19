using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Application.Contracts.Dtos;
using GaniPay.Expense.Application.Requests;
using GaniPay.Expense.Domain.Entities;

namespace GaniPay.Expense.Application.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepo;

    public ExpenseService(IExpenseRepository expenseRepo)
    {
        _expenseRepo = expenseRepo;
    }

    public async Task<List<ExpenseDto>> ListAsync(CancellationToken ct)
    {
        var list = await _expenseRepo.ListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<ExpenseDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        ValidateCreate(request);

        var existing = await _expenseRepo.GetByCodeAsync(request.Code, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Bu code zaten var: {request.Code}");

        var entity = new ExpenseDefinition(
            id: Guid.NewGuid(),
            code: request.Code,
            name: request.Name,
            description: request.Description,
            minAmount: request.MinAmount,
            maxAmount: request.MaxAmount,
            percent: request.Percent,
            fixedAmount: request.FixedAmount,
            currency: request.Currency,
            isVisible: request.IsVisible);

        await _expenseRepo.AddAsync(entity, ct);
        return Map(entity);
    }

    public async Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id boþ olamaz.", nameof(id));
        ValidateUpdate(request);

        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new KeyNotFoundException("Expense bulunamadý.");

        entity.Update(
            name: request.Name,
            description: request.Description,
            minAmount: request.MinAmount,
            maxAmount: request.MaxAmount,
            percent: request.Percent,
            fixedAmount: request.FixedAmount,
            currency: request.Currency,
            isVisible: request.IsVisible);

        await _expenseRepo.UpdateAsync(entity, ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id boþ olamaz.", nameof(id));

        var entity = await _expenseRepo.GetByIdAsync(id, ct);
        if (entity is null) throw new KeyNotFoundException("Expense bulunamadý.");

        await _expenseRepo.DeleteAsync(entity, ct);
    }

    public async Task<decimal> CalculateAsync(CalculateExpenseRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Code boþ olamaz.", nameof(request.Code));

        if (request.Amount < 0)
            throw new ArgumentOutOfRangeException(nameof(request.Amount), "Amount negatif olamaz.");

        var entity = await _expenseRepo.GetByCodeAsync(request.Code, ct);
        if (entity is null) throw new KeyNotFoundException("Expense code bulunamadý.");

        // currency eþleþmesi MVP’de opsiyonel; istersen strict yaparýz.
        return entity.CalculateFee(request.Amount);
    }

    private static ExpenseDto Map(ExpenseDefinition e) => new()
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
        IsVisible = e.IsVisible,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static void ValidateCreate(CreateExpenseRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Code)) throw new ArgumentException("Code boþ olamaz.");
        if (string.IsNullOrWhiteSpace(r.Name)) throw new ArgumentException("Name boþ olamaz.");

        if (r.Percent.HasValue && r.Percent.Value < 0) throw new ArgumentException("Percent negatif olamaz.");
        if (r.FixedAmount.HasValue && r.FixedAmount.Value < 0) throw new ArgumentException("FixedAmount negatif olamaz.");
        if (r.MinAmount.HasValue && r.MinAmount.Value < 0) throw new ArgumentException("MinAmount negatif olamaz.");
        if (r.MaxAmount.HasValue && r.MaxAmount.Value < 0) throw new ArgumentException("MaxAmount negatif olamaz.");
        if (r.MinAmount.HasValue && r.MaxAmount.HasValue && r.MinAmount.Value > r.MaxAmount.Value)
            throw new ArgumentException("MinAmount, MaxAmount'dan büyük olamaz.");
    }

    private static void ValidateUpdate(UpdateExpenseRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) throw new ArgumentException("Name boþ olamaz.");

        if (r.Percent.HasValue && r.Percent.Value < 0) throw new ArgumentException("Percent negatif olamaz.");
        if (r.FixedAmount.HasValue && r.FixedAmount.Value < 0) throw new ArgumentException("FixedAmount negatif olamaz.");
        if (r.MinAmount.HasValue && r.MinAmount.Value < 0) throw new ArgumentException("MinAmount negatif olamaz.");
        if (r.MaxAmount.HasValue && r.MaxAmount.Value < 0) throw new ArgumentException("MaxAmount negatif olamaz.");
        if (r.MinAmount.HasValue && r.MaxAmount.HasValue && r.MinAmount.Value > r.MaxAmount.Value)
            throw new ArgumentException("MinAmount, MaxAmount'dan büyük olamaz.");
    }
}
