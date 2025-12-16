using GaniPay.Identity.Application.Contracts.Dtos;
using GaniPay.Identity.Application.Contracts.Requests;

namespace GaniPay.Identity.Application.Services;

public interface IIdentityService
{
    CredentialDto StartRegistration(StartRegistrationRequest req);
    CredentialDto CompleteRegistration(CompleteRegistrationRequest req);

    CredentialDto? GetCredentialById(Guid id);
    CredentialDto? GetCredentialByPhone(string phoneNumber);

    // Login: phone + password
    CredentialDto Login(LoginRequest req);

    // Recovery
    (CredentialRecoveryDto recovery, string plainToken) StartRecovery(StartRecoveryRequest req);
    void CompleteRecovery(CompleteRecoveryRequest req);
}
