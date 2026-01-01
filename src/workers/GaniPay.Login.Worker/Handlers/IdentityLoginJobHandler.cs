using GaniPay.Login.Worker.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Login.Worker.Handlers;

public sealed class IdentityLoginJobHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IdentityApiOptions _identityOpt;

    public IdentityLoginJobHandler(IHttpClientFactory httpClientFactory, IOptions<IdentityApiOptions> identityOpt)
    {
        _httpClientFactory = httpClientFactory;
        _identityOpt = identityOpt.Value;
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        // zb-client: job.Variables JSON string
        using var doc = JsonDocument.Parse(job.Variables);

        string? phoneNumber = doc.RootElement.TryGetProperty("phoneNumber", out var pn) ? pn.GetString() : null;
        string? password = doc.RootElement.TryGetProperty("password", out var pw) ? pw.GetString() : null;

        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
        {
            var varsJson = JsonSerializer.Serialize(new
            {
                ok = false,
                loginOk = false,
                errorCode = "AUTH_INVALID_INPUT"
            });

            await client.NewCompleteJobCommand(job.Key)
                .Variables(varsJson)
                .Send();

            return;
        }

        // PrepareContext 90 eklediyse kırp (Identity API 10 hane bekliyorsa)
        if (phoneNumber.StartsWith("90") && phoneNumber.Length > 10)
            phoneNumber = phoneNumber.Substring(2);

        try
        {
            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(_identityOpt.BaseUrl.TrimEnd('/'));

            var req = new { phoneNumber, password };
            var resp = await http.PostAsJsonAsync("/api/v1/identity/login", req);

            if (!resp.IsSuccessStatusCode)
            {
                var varsJson = JsonSerializer.Serialize(new
                {
                    ok = false,
                    loginOk = false,
                    errorCode = $"IDENTITY_HTTP_{(int)resp.StatusCode}"
                });

                await client.NewCompleteJobCommand(job.Key)
                    .Variables(varsJson)
                    .Send();

                return;
            }

            var body = await resp.Content.ReadFromJsonAsync<IdentityLoginResponse>();

            if (body is null || body.Success is false)
            {
                var varsJson = JsonSerializer.Serialize(new
                {
                    ok = false,
                    loginOk = false,
                    errorCode = body?.ErrorCode ?? "IDENTITY_LOGIN_FAILED"
                });

                await client.NewCompleteJobCommand(job.Key)
                    .Variables(varsJson)
                    .Send();

                return;
            }

            // Success
            var successVarsJson = JsonSerializer.Serialize(new
            {
                ok = true,
                loginOk = true,
                customerId = body.CustomerId,
                errorCode = (string?)null
            });

            await client.NewCompleteJobCommand(job.Key)
                .Variables(successVarsJson)
                .Send();
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