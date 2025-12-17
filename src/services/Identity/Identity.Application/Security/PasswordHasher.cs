using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace GaniPay.Identity.Application.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const string Algo = "PBKDF2-HMACSHA256";
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public (string hash, string salt, string algo) Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

        var key = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );

        return (Convert.ToBase64String(key), Convert.ToBase64String(saltBytes), Algo);
    }

    public bool Verify(string password, string hash, string salt, string algo)
    {
        if (algo != Algo) return false;
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (string.IsNullOrWhiteSpace(hash)) return false;
        if (string.IsNullOrWhiteSpace(salt)) return false;

        var saltBytes = Convert.FromBase64String(salt);

        var key = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );

        var computed = Convert.ToBase64String(key);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hash),
            Convert.FromBase64String(computed)
        );
    }
}
