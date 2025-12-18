using GaniPay.TransactionLimit.Domain.Entities;

namespace GaniPay.TransactionLimit.Application.Abstractions;

public interface ILimitDefinitionRepository
{
    Task<LimitDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LimitDefinition?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<LimitDefinition>> ListAsync(CancellationToken ct = default);
    Task AddAsync(LimitDefinition entity, CancellationToken ct = default);
    Task UpdateAsync(LimitDefinition entity, CancellationToken ct = default);
    Task DeleteAsync(LimitDefinition entity, CancellationToken ct = default);
}
