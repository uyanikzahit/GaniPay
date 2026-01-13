using GaniPay.Login.Worker.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Login.Worker.Handlers;

public sealed class IdentityLoginJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IdentityApiOptions _identityOpt;

    public IdentityLoginJobHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityApiOptions> identityOpt)
    {
        _httpClientFactory = httpClientFactory;
        _identityOpt = identityOpt.Value;
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        Console.WriteLine("IdentityLoginJobHandler vNEXT running"); // ✅ worker yeni mi kontrol

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(job.Variables);
        }
        catch (Exception ex)
        {
            var nextRetries = Math.Max(job.Retries - 1, 0);
            await client.NewFailCommand(job.Key)
                .Retries(nextRetries)
                .ErrorMessage($"Invalid job variables JSON: {ex.Message}")
                .Send();
            return;
        }

        using (doc)
        {
            var root = doc.RootElement;

            var phoneNumber =
                TryGetString(root, "normalizedPhoneNumber")
                ?? TryGetString(root, "phoneNumber");

            var password = TryGetString(root, "password");

            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                await CompleteAsync(client, job, new
                {
                    ok = false,
                    loginOk = false,
                    errorCode = "AUTH_INVALID_INPUT",
                    phoneNumberForIdentity = (string?)null
                });
                return;
            }

            // ✅ Identity API'nin kabul ettiği format: 0 ile başlayan 11 hane (0506...)
            var phoneForIdentity = NormalizeForIdentity(phoneNumber);
            Console.WriteLine($"Identity login sending phoneNumber={phoneForIdentity}");

            try
            {
                var baseUrl = (_identityOpt.BaseUrl ?? "http://localhost:5102").TrimEnd('/');
                var http = _httpClientFactory.CreateClient();
                http.BaseAddress = new Uri(baseUrl);

                var req = new { phoneNumber = phoneForIdentity, password };
                var resp = await http.PostAsJsonAsync("/api/v1/identity/login", req, JsonOpts);

                if (!resp.IsSuccessStatusCode)
                {
                    var raw = await resp.Content.ReadAsStringAsync();

                    await CompleteAsync(client, job, new
                    {
                        ok = false,
                        loginOk = false,
                        errorCode = $"IDENTITY_HTTP_{(int)resp.StatusCode}",
                        phoneNumberForIdentity = phoneForIdentity,
                        identityError = raw
                    });

                    return;
                }

                var body = await resp.Content.ReadFromJsonAsync<IdentityLoginResponse>(JsonOpts);

                if (body is null || body.Success is false)
                {
                    await CompleteAsync(client, job, new
                    {
                        ok = false,
                        loginOk = false,
                        errorCode = body?.ErrorCode ?? "IDENTITY_LOGIN_FAILED",
                        phoneNumberForIdentity = phoneForIdentity
                    });
                    return;
                }

                // ✅ Success (Modeler output’larıyla uyumlu)
                await CompleteAsync(client, job, new
                {
                    ok = true,
                    loginOk = true,
                    customerId = body.CustomerId,
                    accessToken = body.AccessToken,
                    expiresIn = body.ExpiresIn,
                    errorCode = (string?)null,
                    phoneNumberForIdentity = phoneForIdentity
                });
            }
            catch (Exception ex)
            {
                var nextRetries = Math.Max(job.Retries - 1, 0);

                await client.NewFailCommand(job.Key)
                    .Retries(nextRetries)
                    .ErrorMessage(ex.Message)
                    .Send();
            }
        }
    }

    /// <summary>
    /// Identity API sadece 0 ile başlayan 11 hane formatını kabul ediyor (0506...).
    /// Gelen değer 90'lı / 0'suz olsa bile bunu garanti eder.
    /// </summary>
    private static string NormalizeForIdentity(string pn)
    {
        // sadece rakamları al
        var digits = new string(pn.Where(char.IsDigit).ToArray());

        // 90 ile başlıyorsa kırp (90506... -> 506...)
        if (digits.StartsWith("90"))
            digits = digits.Substring(2);

        // 10 haneye indir (5062533600)
        if (digits.Length > 10)
            digits = digits.Substring(digits.Length - 10);

        // 0 ile başlat (0506...)
        if (!digits.StartsWith("0"))
            digits = "0" + digits;

        return digits;
    }

    private static async Task CompleteAsync(IJobClient client, IJob job, object variables)
    {
        var varsJson = JsonSerializer.Serialize(variables, JsonOpts);

        await client.NewCompleteJobCommand(job.Key)
            .Variables(varsJson)
            .Send();
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return null;

        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString(),
            JsonValueKind.Number => prop.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => prop.GetRawText()
        };
    }
}
