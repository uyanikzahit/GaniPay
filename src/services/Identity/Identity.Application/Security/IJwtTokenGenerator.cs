namespace GaniPay.Identity.Application.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid customerId, string? phone, string role);
}
