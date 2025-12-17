using System.Security.Cryptography;
using System.Text;
using GaniPay.Identity.Application.Abstractions;
using GaniPay.Identity.Application.Security;
using GaniPay.Identity.Domain.Entities;
using GaniPay.Identity.Domain.Enums;

namespace GaniPay.Identity.Application.Services;

public sealed class IdentityService
{
    private readonly IPasswordHasher _hasher;
    private readonly ICredentialRepository _credentialRepo;
    private readonly ICredentialRecoveryRepository _recoveryRepo;

    public IdentityService(
        IPasswordHasher hasher,
        ICredentialRepository credentialRepo,
        ICredentialRecoveryRepository recoveryRepo)
    {
        _hasher = hasher;
        _credentialRepo = credentialRepo;
        _recoveryRepo = recoveryRepo;
    }

    public async Task<Credential> StartRegistrationAsync(Guid customerId, string phoneNumber, string password, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        var loginType = "phone";
        var loginValue = NormalizePhone(phoneNumber);
        Require(loginValue, nameof(phoneNumber));
        Require(password, nameof(password));

        if (await _credentialRepo.ExistsByLoginAsync(loginType, loginValue, ct))
            throw new InvalidOperationException("Phone number already registered.");

        var now = DateTime.UtcNow;
        var (hash, salt, algo) = _hasher.Hash(password);

        var entity = new Credential
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            LoginType = loginType,
            LoginValue = loginValue,
            PasswordHash = hash,
            PasswordSalt = salt,
            PasswordAlgo = algo,
            FailedLoginCount = 0,
            IsLocked = false,
            LockReason = null,
            LastLoginAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _credentialRepo.AddAsync(entity, ct);
        await _credentialRepo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<Credential> LoginAsync(string phoneNumber, string password, CancellationToken ct = default)
    {
        var loginType = "phone";
        var loginValue = NormalizePhone(phoneNumber);
        Require(loginValue, nameof(phoneNumber));
        Require(password, nameof(password));

        var entity = await _credentialRepo.GetByLoginAsync(loginType, loginValue, ct)
                     ?? throw new UnauthorizedAccessException("Invalid phone or password.");

        if (entity.IsLocked)
            throw new UnauthorizedAccessException("Credential locked.");

        if (!_hasher.Verify(password, entity.PasswordHash, entity.PasswordSalt, entity.PasswordAlgo))
        {
            entity.FailedLoginCount += 1;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entity.FailedLoginCount >= 5)
            {
                entity.IsLocked = true;
                entity.LockReason = "Too many failed login attempts.";
            }

            await _credentialRepo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid phone or password.");
        }

        entity.FailedLoginCount = 0;
        entity.IsLocked = false;
        entity.LockReason = null;
        entity.LastLoginAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _credentialRepo.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<(CredentialRecovery recovery, string plainToken)> StartRecoveryAsync(
        string phoneNumber,
        RecoveryChannel channel,
        CancellationToken ct = default)
    {
        var loginType = "phone";
        var loginValue = NormalizePhone(phoneNumber);
        Require(loginValue, nameof(phoneNumber));

        var cred = await _credentialRepo.GetByLoginAsync(loginType, loginValue, ct)
                   ?? throw new KeyNotFoundException("Credential not found.");

        var now = DateTime.UtcNow;

        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Sha256Base64(plainToken);

        var entity = new CredentialRecovery
        {
            Id = Guid.NewGuid(),
            CredentialId = cred.Id,
            Channel = channel,
            TokenHash = tokenHash,
            ExpiresAt = now.AddMinutes(15),
            UsedAt = null,
            CreatedAt = now
        };

        await _recoveryRepo.AddAsync(entity, ct);
        await _recoveryRepo.SaveChangesAsync(ct);

        return (entity, plainToken);
    }

    public async Task CompleteRecoveryAsync(string token, string newPassword, CancellationToken ct = default)
    {
        Require(token, nameof(token));
        Require(newPassword, nameof(newPassword));

        var tokenHash = Sha256Base64(token);

        var rec = await _recoveryRepo.GetByTokenHashAsync(tokenHash, ct)
                  ?? throw new UnauthorizedAccessException("Invalid token.");

        if (rec.UsedAt.HasValue)
            throw new UnauthorizedAccessException("Token already used.");

        if (rec.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token expired.");

        var cred = await _credentialRepo.GetByIdAsync(rec.CredentialId, ct)
                   ?? throw new KeyNotFoundException("Credential not found.");

        var (hash, salt, algo) = _hasher.Hash(newPassword);

        cred.PasswordHash = hash;
        cred.PasswordSalt = salt;
        cred.PasswordAlgo = algo;
        cred.UpdatedAt = DateTime.UtcNow;

        rec.UsedAt = DateTime.UtcNow;

        await _credentialRepo.SaveChangesAsync(ct);
        await _recoveryRepo.SaveChangesAsync(ct);
    }

    private static void Require(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} is required.");
    }

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "";
        return phone.Trim().Replace(" ", "");
    }

    private static string Sha256Base64(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
