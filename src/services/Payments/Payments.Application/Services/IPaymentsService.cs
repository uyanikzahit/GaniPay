using GaniPay.Payments.Application.Contracts.Dtos;
using GaniPay.Payments.Application.Contracts.Requests;

namespace GaniPay.Payments.Application.Services;

public interface IPaymentsService
{
    Task<StartPaymentResultDto> StartTransferAsync(StartTransferRequest request, CancellationToken ct);
    Task<StartPaymentResultDto> StartTopUpAsync(StartTopUpRequest request, CancellationToken ct);
    Task<PaymentProcessDto> GetStatusAsync(GetPaymentStatusRequest request, CancellationToken ct);
}
