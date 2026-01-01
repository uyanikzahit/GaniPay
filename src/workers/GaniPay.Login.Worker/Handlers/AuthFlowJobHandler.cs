using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Login.Worker.Handlers;

public sealed class AuthFlowJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task Handle(IJobClient client, IJob job)
    {
        // job.Variables JSON string
        var vars = ParseVars(job.Variables);

        // job.Type hangi job type geldi?
        var jobType = job.Type;

        // Çıkış değişkenleri
        object output = jobType switch
        {
            "mock.auth.context.prepare" => PrepareContext(vars),
            "mock.auth.bruteforce.guard" => BruteForceGuard(vars),
            "mock.auth.account.status" => AccountStatus(vars),
            "mock.auth.device.trust" => DeviceTrust(vars),
            "mock.auth.session.create" => CreateSession(vars),
            "mock.auth.audit.log" => AuditLog(vars),

            // yanlış job type gelirse kırma, fail’e düşür (ok=false)
            _ => new { ok = false, errorCode = "UNSUPPORTED_JOB_TYPE" }
        };

        var json = JsonSerializer.Serialize(output, JsonOpts);

        await client.NewCompleteJobCommand(job.Key)
            .Variables(json)
            .Send();
    }

    // ---- MOCK LOGIC (basit, deterministik) ----

    private static object PrepareContext(Dictionary<string, object?> v)
    {
        var phone = GetString(v, "phoneNumber") ?? "";
        var normalized = NormalizePhone(phone);

        var correlationId = Guid.NewGuid().ToString("N");

        return new
        {
            correlationId,
            normalizedPhoneNumber = normalized
        };
    }

    private static object BruteForceGuard(Dictionary<string, object?> v)
    {
        // Basit mock:
        // password = "wrong" içeriyorsa guard fail gibi davran, yoksa ok
        var password = GetString(v, "password") ?? "";
        var ok = !password.Contains("wrong", StringComparison.OrdinalIgnoreCase);

        return new
        {
            ok,
            attemptCount = ok ? 0 : 3,
            lockoutUntil = ok ? (string?)null : DateTime.UtcNow.AddMinutes(15).ToString("O"),
            errorCode = ok ? (string?)null : "AUTH_LOCKED"
        };
    }

    private static object AccountStatus(Dictionary<string, object?> v)
    {
        // Mock account status
        // customerId boşsa fail
        var customerId = GetString(v, "customerId");
        var ok = !string.IsNullOrWhiteSpace(customerId);

        return new
        {
            ok,
            status = ok ? "ACTIVE" : "BLOCKED",
            errorCode = ok ? (string?)null : "ACCOUNT_NOT_FOUND"
        };
    }

    private static object DeviceTrust(Dictionary<string, object?> v)
    {
        // Mock device trust
        var deviceId = GetString(v, "deviceId") ?? "";
        var trusted = deviceId.EndsWith("T", StringComparison.OrdinalIgnoreCase); // örnek kural
        var risk = trusted ? "LOW" : "HIGH";

        return new
        {
            ok = true, // mock'ta cihaz kontrolü çoğu zaman ok
            riskLevel = risk,
            trustedDevice = trusted,
            errorCode = (string?)null
        };
    }

    private static object CreateSession(Dictionary<string, object?> v)
    {
        // Mock session create
        var accessToken = "mock_access_" + Guid.NewGuid().ToString("N");
        var refreshToken = "mock_refresh_" + Guid.NewGuid().ToString("N");

        return new
        {
            accessToken,
            refreshToken,
            expiresIn = 3600,
            sessionId = Guid.NewGuid().ToString("N")
        };
    }

    private static object AuditLog(Dictionary<string, object?> v)
    {
        // Mock audit log (db yoksa bile "ok=true" dön)
        // success değişkeni gelmişse aynen geçir, yoksa true
        var success = GetBool(v, "success") ?? true;

        return new
        {
            ok = true,
            success,
            errorCode = GetString(v, "errorCode")
        };
    }

    // ---- Helpers ----

    private static Dictionary<string, object?> ParseVars(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, object?>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOpts)
                   ?? new Dictionary<string, object?>();
        }
        catch
        {
            return new Dictionary<string, object?>();
        }
    }

    private static string? GetString(Dictionary<string, object?> v, string key)
    {
        if (!v.TryGetValue(key, out var val) || val is null) return null;

        // System.Text.Json bazen JsonElement getirir
        if (val is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => je.GetRawText()
            };
        }

        return val.ToString();
    }

    private static bool? GetBool(Dictionary<string, object?> v, string key)
    {
        if (!v.TryGetValue(key, out var val) || val is null) return null;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.True) return true;
            if (je.ValueKind == JsonValueKind.False) return false;
            if (je.ValueKind == JsonValueKind.String && bool.TryParse(je.GetString(), out var b)) return b;
        }

        if (val is bool bb) return bb;
        if (bool.TryParse(val.ToString(), out var b2)) return b2;

        return null;
    }

    private static string NormalizePhone(string phone)
    {
        // sadece rakamları al
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // 90 ekle (çok basit)
        if (digits.Length == 10) digits = "90" + digits;
        if (digits.StartsWith("0") && digits.Length == 11) digits = "9" + digits;

        return digits;
    }
}