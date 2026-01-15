using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Login.Worker.Handlers;

public sealed class AuthFlowJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly string _workflowBaseUrl;

    public AuthFlowJobHandler(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _workflowBaseUrl = (cfg["WorkflowApi:BaseUrl"] ?? "https://localhost:7253").TrimEnd('/');
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        // job.Variables JSON string
        var vars = ParseVars(job.Variables);

        // job.Type hangi job type geldi?
        var jobType = job.Type;

        // Çıkış değişkenleri
        object output;

        switch (jobType)
        {
            case "mock.auth.context.prepare":
                output = PrepareContext(vars);
                break;

            case "mock.auth.bruteforce.guard":
                output = BruteForceGuard(vars);
                break;

            case "mock.auth.device.trust":
                output = DeviceTrust(vars);
                break;

            case "mock.auth.session.create":
                output = CreateSession(vars);
                break;

            case "mock.auth.audit.log":
                output = await AuditLogAsync(vars);
                break;

            // yanlış job type gelirse kırma, fail’e düşür (ok=false)
            default:
                output = new { ok = false, errorCode = "UNSUPPORTED_JOB_TYPE" };
                break;
        }

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

        // ✅ KRİTİK: correlationId'yi EZME!
        var correlationId =
            GetString(v, "correlationId")
            ?? Guid.NewGuid().ToString();

        return new
        {
            correlationId,
            normalizedPhoneNumber = normalized
        };
    }

    private static object BruteForceGuard(Dictionary<string, object?> v)
    {
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
        var deviceId = GetString(v, "deviceId") ?? "";
        var trusted = deviceId.EndsWith("T", StringComparison.OrdinalIgnoreCase);
        var risk = trusted ? "LOW" : "HIGH";

        return new
        {
            ok = true,
            riskLevel = risk,
            trustedDevice = trusted,
            errorCode = (string?)null
        };
    }

    // ✅ TOKEN FIX: mock token üretme, process'ten gelen gerçek accessToken/refreshToken'ı taşı
    private static object CreateSession(Dictionary<string, object?> v)
    {
        // Identity Login adımından gelen token'lar
        var accessToken = GetString(v, "accessToken");
        var refreshToken = GetString(v, "refreshToken");

        // expiresIn varsa taşı (string/number olabilir)
        var expiresInStr = GetString(v, "expiresIn");
        var expiresIn = int.TryParse(expiresInStr, out var sec) ? sec : 3600;

        return new
        {
            ok = true,
            errorCode = (string?)null,

            // ✅ overwrite etmez: mock üretmiyoruz
            accessToken,
            refreshToken,

            expiresIn,
            sessionId = Guid.NewGuid().ToString("N")
        };
    }

    private async Task<object> AuditLogAsync(Dictionary<string, object?> v)
    {
        // ✅ Workflow sonucunu çıkar
        var correlationId = GetString(v, "correlationId") ?? "";

        // ✅ gerçek token (CreateSession artık mock üretmediği için burada gerçek gelir)
        var token = GetString(v, "accessToken");

        var success = !string.IsNullOrWhiteSpace(token);
        var status = success ? "Succeeded" : "Failed";
        var message = success ? "Login successful" : "The password or phone number is incorrect.";

        // ✅ Akışta üretilen dataları da callback’e taşı
        v.TryGetValue("customer", out var customerObj);
        v.TryGetValue("wallets", out var walletsObj);

        var payload = new
        {
            correlationId,
            success,
            status,
            message,
            token,

            // ekstra alanlar
            customerId = GetString(v, "customerId"),
            customer = customerObj,
            wallets = walletsObj
        };

        // ✅ Callback at
        var url = $"{_workflowBaseUrl}/api/v1/auth/login/result";

        try
        {
            Console.WriteLine($"[AuthFlow] login/result callback -> {url} | correlationId={correlationId}");

            var resp = await _http.PostAsJsonAsync(url, payload);
            var raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AuthFlow] CALLBACK HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} | body={raw}");
            }
            else
            {
                Console.WriteLine($"[AuthFlow] CALLBACK OK | body={raw}");
            }

            resp.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // ❗ önceki sürüm bunu yutuyordu, şimdi görelim
            Console.WriteLine($"[AuthFlow] CALLBACK FAILED: {ex.GetType().Name} - {ex.Message}");
        }

        return new
        {
            ok = true,
            success,
            errorCode = success ? (string?)null : "LOGIN_FAILED"
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
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.Length == 10) digits = "90" + digits;
        if (digits.StartsWith("0") && digits.Length == 11) digits = "9" + digits;

        return digits;
    }
}
