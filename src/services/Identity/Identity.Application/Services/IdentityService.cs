using System.Security.Cryptography;
using System.Text;
using GaniPay.Identity.Application.Contracts.Dtos;
using GaniPay.Identity.Application.Contracts.Enums;
using GaniPay.Identity.Application.Contracts.Requests;
using GaniPay.Identity.Application.Security;
using GaniPay.Identity.Domain.Entities;
using GaniPay.Identity.Domain.Enums;

namespace GaniPay.Identity.Application.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly IPasswordHasher _hasher;

    // MVP: In-memory stores
    private readonly Dictionary<Guid, Credential> _credentialsById = new();
    private readonly Dictionary<string, Guid> _credentialIdByPhone = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<Guid, CredentialRecovery> _recoveriesById = new();
    private readonly Dictionary<string, Guid> _recoveryIdByTokenHash = new(StringComparer.Ordinal);

    public IdentityService(IPasswordHasher hasher)
    {
        _hasher = hasher;
    }

    public CredentialDto StartRegistration(StartRegistrationRequest req)
    {
        if (req.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.");

        var phone = NormalizePhone(req.PhoneNumber);
        Require(phone, "PhoneNumber");
        Require(req.Password, "Password");

        if (_credentialIdByPhone.ContainsKey(phone))
            throw new InvalidOperationException("Phone number already registered.");

        var now = DateTime.UtcNow;

        var entity = new Credential
        {
            Id = Guid.NewGuid(),
            CustomerId = req.CustomerId,
            PhoneNumber = phone,
            Email = string.Empty,
            PasswordHash = _hasher.Hash(req.Password),

            FailedLoginCount = 0,
            Status = Domain.Enums.CredentialStatus.Active,
            LockoutEndAt = null,
            LockReason = null,

            LastLoginAt = null,
            PhoneVerifiedAt = null,
            EmailVerifiedAt = null,

            // 2 adım: phone+password alındı -> email bekleniyor
            RegistrationStatus = Domain.Enums.RegistrationStatus.PendingEmail,

            CreatedAt = now,
            UpdatedAt = now
        };

        _credentialsById[entity.Id] = entity;
        _credentialIdByPhone[phone] = entity.Id;

        return ToDto(entity);
    }

    public CredentialDto CompleteRegistration(CompleteRegistrationRequest req)
    {
        if (req.CredentialId == Guid.Empty)
            throw new ArgumentException("CredentialId is required.");

        var email = NormalizeEmail(req.Email);
        Require(email, "Email");

        if (!_credentialsById.TryGetValue(req.CredentialId, out var entity))
            throw new KeyNotFoundException("Credential not found.");

        var now = DateTime.UtcNow;

        entity.Email = email;
        entity.RegistrationStatus = Domain.Enums.RegistrationStatus.Completed;
        entity.EmailVerifiedAt = null; // MVP: email verify yok
        entity.UpdatedAt = now;

        return ToDto(entity);
    }

    public CredentialDto? GetCredentialById(Guid id)
    {
        return _credentialsById.TryGetValue(id, out var e) ? ToDto(e) : null;
    }

    public CredentialDto? GetCredentialByPhone(string phoneNumber)
    {
        var phone = NormalizePhone(phoneNumber);
        if (_credentialIdByPhone.TryGetValue(phone, out var id))
            return GetCredentialById(id);

        return null;
    }

    public CredentialDto Login(LoginRequest req)
    {
        var phone = NormalizePhone(req.PhoneNumber);
        Require(phone, "PhoneNumber");
        Require(req.Password, "Password");

        if (!_credentialIdByPhone.TryGetValue(phone, out var id))
            throw new UnauthorizedAccessException("Invalid phone or password.");

        var entity = _credentialsById[id];

        // Domain enum ile kontrol
        if (entity.Status == Domain.Enums.CredentialStatus.Disabled)
            throw new UnauthorizedAccessException("Credential disabled.");

        if (entity.Status == Domain.Enums.CredentialStatus.Locked &&
            entity.LockoutEndAt.HasValue &&
            entity.LockoutEndAt.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Credential locked.");
        }

        if (!_hasher.Verify(req.Password, entity.PasswordHash))
        {
            entity.FailedLoginCount += 1;
            entity.UpdatedAt = DateTime.UtcNow;

            // Basit lockout: 5 deneme -> 5dk
            if (entity.FailedLoginCount >= 5)
            {
                entity.Status = Domain.Enums.CredentialStatus.Locked;
                entity.LockoutEndAt = DateTime.UtcNow.AddMinutes(5);
                entity.LockReason = "Too many failed login attempts.";
            }

            throw new UnauthorizedAccessException("Invalid phone or password.");
        }

        // başarılı login
        entity.FailedLoginCount = 0;
        entity.Status = Domain.Enums.CredentialStatus.Active;
        entity.LockoutEndAt = null;
        entity.LockReason = null;
        entity.LastLoginAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        return ToDto(entity);
    }

    public (CredentialRecoveryDto recovery, string plainToken) StartRecovery(StartRecoveryRequest req)
    {
        var phone = NormalizePhone(req.PhoneNumber);
        Require(phone, "PhoneNumber");

        if (!_credentialIdByPhone.TryGetValue(phone, out var credentialId))
            throw new KeyNotFoundException("Credential not found.");

        var now = DateTime.UtcNow;

        // demo token üret, db'ye hash yaz
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Sha256Base64(plainToken);

        // Contracts enum -> Domain enum cast
        var channelDomain = (Domain.Enums.RecoveryChannel)req.Channel;

        var entity = new CredentialRecovery
        {
            Id = Guid.NewGuid(),
            CredentialId = credentialId,
            Channel = channelDomain,
            TokenHash = tokenHash,
            ExpiresAt = now.AddMinutes(15),
            UsedAt = null,
            CreatedAt = now
        };

        _recoveriesById[entity.Id] = entity;
        _recoveryIdByTokenHash[tokenHash] = entity.Id;

        return (ToDto(entity), plainToken);
    }

    public void CompleteRecovery(CompleteRecoveryRequest req)
    {
        Require(req.Token, "Token");
        Require(req.NewPassword, "NewPassword");

        var tokenHash = Sha256Base64(req.Token);

        if (!_recoveryIdByTokenHash.TryGetValue(tokenHash, out var recoveryId))
            throw new UnauthorizedAccessException("Invalid token.");

        var recovery = _recoveriesById[recoveryId];

        if (recovery.UsedAt.HasValue)
            throw new UnauthorizedAccessException("Token already used.");

        if (recovery.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token expired.");

        if (!_credentialsById.TryGetValue(recovery.CredentialId, out var cred))
            throw new KeyNotFoundException("Credential not found.");

        cred.PasswordHash = _hasher.Hash(req.NewPassword);
        cred.UpdatedAt = DateTime.UtcNow;

        recovery.UsedAt = DateTime.UtcNow;
    }

    // -------------------------
    // Mapping (Domain -> Contract DTO)
    // -------------------------
    private static CredentialDto ToDto(Credential e) => new()
    {
        Id = e.Id,
        CustomerId = e.CustomerId,
        PhoneNumber = e.PhoneNumber,
        Email = e.Email,

        FailedLoginCount = e.FailedLoginCount,

        // Domain enum -> Contracts enum cast
        Status = (GaniPay.Identity.Application.Contracts.Enums.CredentialStatus)e.Status,
        RegistrationStatus = (GaniPay.Identity.Application.Contracts.Enums.RegistrationStatus)e.RegistrationStatus,

        LockoutEndAt = e.LockoutEndAt,
        LockReason = e.LockReason,

        LastLoginAt = e.LastLoginAt,
        PhoneVerifiedAt = e.PhoneVerifiedAt,
        EmailVerifiedAt = e.EmailVerifiedAt,

        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static CredentialRecoveryDto ToDto(CredentialRecovery r) => new()
    {
        Id = r.Id,
        CredentialId = r.CredentialId,

        // Domain enum -> Contracts enum cast
        Channel = (GaniPay.Identity.Application.Contracts.Enums.RecoveryChannel)r.Channel,

        ExpiresAt = r.ExpiresAt,
        UsedAt = r.UsedAt,
        CreatedAt = r.CreatedAt
    };

    // -------------------------
    // Helpers
    // -------------------------
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

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "";
        return email.Trim().ToLowerInvariant();
    }

    private static string Sha256Base64(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
