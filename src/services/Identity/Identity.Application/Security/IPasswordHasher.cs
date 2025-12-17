namespace GaniPay.Identity.Application.Security;

public interface IPasswordHasher
{
    (string hash, string salt, string algo) Hash(string password);
    bool Verify(string password, string hash, string salt, string algo);
}
