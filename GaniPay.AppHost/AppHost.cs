using Aspire.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

static void OpenBrowser(string url)
{
    try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
    catch { /* ignore */ }
}

static void HookDashboardAutoOpen()
{
    var originalOut = Console.Out;
    var originalErr = Console.Error;

    int opened = 0;

    void TryOpen(string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;

        const string marker = "Login to the dashboard at ";
        var idx = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return;

        var url = line[(idx + marker.Length)..].Trim();
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;

        if (Interlocked.Exchange(ref opened, 1) == 0)
            OpenBrowser(url);
    }

    Console.SetOut(new AutoOpenWriter(originalOut, TryOpen));
    Console.SetError(new AutoOpenWriter(originalErr, TryOpen));
}

HookDashboardAutoOpen();

var builder = DistributedApplication.CreateBuilder(args);

// -------------------- APIs (mevcutlarını BOZMADAN) --------------------
var accountingApi = builder.AddProject<Projects.GaniPay_Accounting_API>("accounting-api")
    .WithExternalHttpEndpoints();

var customerApi = builder.AddProject<Projects.GaniPay_Customer_API>("customer-api")
    .WithExternalHttpEndpoints();

var expenseApi = builder.AddProject<Projects.GaniPay_Expense_API>("expense-api")
    .WithExternalHttpEndpoints();

var identityApi = builder.AddProject<Projects.GaniPay_Identity_API>("identity-api")
    .WithExternalHttpEndpoints();

var integrationApi = builder.AddProject<Projects.GaniPay_Integration_API>("integration-api")
    .WithExternalHttpEndpoints();

var notificationApi = builder.AddProject<Projects.GaniPay_Notification_API>("notification-api")
    .WithExternalHttpEndpoints();

var paymentsApi = builder.AddProject<Projects.GaniPay_Payments_API>("payments-api")
    .WithExternalHttpEndpoints();

var transactionLimitApi = builder.AddProject<Projects.GaniPay_TransactionLimit_API>("transactionlimit-api")
    .WithExternalHttpEndpoints();

// -------------------- WORKERS (Executable) --------------------
// ÖNEMLİ: WorkingDirectory worker klasörü olmalı (appsettings.json doğru okunsun).
var appHostDir = builder.AppHostDirectory;
var repoRoot = Path.GetFullPath(Path.Combine(appHostDir, "..")); // solution root varsayımı
var workersRoot = Path.Combine(repoRoot, "src", "workers");

var dataCreationWorkerDir = Path.Combine(workersRoot, "GaniPay.DataCreation.Worker");
var validationWorkerDir = Path.Combine(workersRoot, "GaniPay.Validation.Worker");

var dataCreationWorkerCsproj = Path.Combine(dataCreationWorkerDir, "GaniPay.DataCreation.Worker.csproj");
var validationWorkerCsproj = Path.Combine(validationWorkerDir, "GaniPay.Validation.Worker.csproj");

var topupWorkerDir = Path.Combine(workersRoot, "GaniPay.TopUp.Worker");
var topupWorkerCsproj = Path.Combine(topupWorkerDir, "GaniPay.TopUp.Worker.csproj");

// Limits Control Worker
var limitsControlWorkerCsproj = Path.Combine(workersRoot, "GaniPay.LimitsControl.Worker", "GaniPay.LimitsControl.Worker.csproj");

builder.AddExecutable("limits-control-worker", "dotnet", Path.GetDirectoryName(limitsControlWorkerCsproj)!)
    .WithArgs("run", "--project", limitsControlWorkerCsproj, "--no-launch-profile")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");


// DataCreation Worker
builder.AddExecutable("data-creation-worker", "dotnet", dataCreationWorkerDir)
    .WithArgs("run", "--project", dataCreationWorkerCsproj, "--no-launch-profile")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    // İstersen bağımlılık olarak API'ler ayakta olsun diye:
    .WaitFor(accountingApi)
    .WaitFor(customerApi)
    .WaitFor(identityApi);

// Validation Worker
builder.AddExecutable("validation-worker", "dotnet", validationWorkerDir)
    .WithArgs("run", "--project", validationWorkerCsproj, "--no-launch-profile")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WaitFor(customerApi)
    .WaitFor(integrationApi);


// -------------------- TRANSFER WORKER --------------------
// workersRoot zaten senin AppHost.cs'inde var: Path.Combine(repoRoot, "src", "workers")

var transferWorkerDir = Path.Combine(workersRoot, "GaniPay.Transfer.Worker");
var transferWorkerCsproj = Path.Combine(transferWorkerDir, "GaniPay.Transfer.Worker.csproj");

builder.AddExecutable("transfer-worker", "dotnet", transferWorkerDir)
    .WithArgs(
        "run",
        "--project", transferWorkerCsproj,
        "--no-launch-profile"
    )
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");



var loginWorkerDir = Path.Combine(workersRoot, "GaniPay.Login.Worker");
var loginWorkerCsproj = Path.Combine(loginWorkerDir, "GaniPay.Login.Worker.csproj");

builder.AddExecutable("login-worker", "dotnet", loginWorkerDir)
    .WithArgs("run", "--project", loginWorkerCsproj, "--no-launch-profile")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

builder.AddExecutable("topup-worker", "dotnet", topupWorkerDir)
    .WithArgs("run", "--project", topupWorkerCsproj, "--no-launch-profile")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

builder.AddProject<Projects.GaniPay_Workflow_API>("ganipay-workflow-api")
    .WithExternalHttpEndpoints();

builder.Build().Run();

sealed class AutoOpenWriter : TextWriter
{
    private readonly TextWriter _inner;
    private readonly Action<string?> _onLine;
    private readonly StringBuilder _buffer = new();

    public AutoOpenWriter(TextWriter inner, Action<string?> onLine)
    {
        _inner = inner;
        _onLine = onLine;
    }

    public override Encoding Encoding => _inner.Encoding;

    public override void Write(char value)
    {
        _inner.Write(value);

        if (value == '\n')
        {
            var line = _buffer.ToString();
            _buffer.Clear();
            _onLine(line);
        }
        else
        {
            _buffer.Append(value);
        }
    }

    public override void Write(string? value)
    {
        _inner.Write(value);
        if (value is null) return;

        if (value.Contains('\n'))
        {
            foreach (var part in value.Split('\n'))
                _onLine(part);
        }
        else
        {
            _onLine(value);
        }
    }

    public override void WriteLine(string? value)
    {
        _inner.WriteLine(value);
        _onLine(value);
    }
}
