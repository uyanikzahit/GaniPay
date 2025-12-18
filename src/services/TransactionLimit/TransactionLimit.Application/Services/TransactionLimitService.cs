using GaniPay.TransactionLimit.Application.Abstractions;

namespace GaniPay.TransactionLimit.Application.Services;

public interface ITransactionLimitService
{
    // Şimdilik boş: bir sonraki adımda CRUD + Check usecase’lerini ekleyeceğiz.
}

public sealed class TransactionLimitService : ITransactionLimitService
{
    private readonly ILimitDefinitionRepository _definitionRepo;
    private readonly ICustomerLimitRepository _customerLimitRepo;

    public TransactionLimitService(
        ILimitDefinitionRepository definitionRepo,
        ICustomerLimitRepository customerLimitRepo)
    {
        _definitionRepo = definitionRepo;
        _customerLimitRepo = customerLimitRepo;
    }
}
