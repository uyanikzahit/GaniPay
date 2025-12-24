using Aspire.Hosting;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

static void OpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch { }
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

// --- APIs ---
builder.AddProject<Projects.GaniPay_Accounting_API>("accounting-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_Customer_API>("customer-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_Expense_API>("expense-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_Identity_API>("identity-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_Integration_API>("integration-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_Notification_API>("notification-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_Payments_API>("payments-api").WithExternalHttpEndpoints();
builder.AddProject<Projects.GaniPay_TransactionLimit_API>("transactionlimit-api").WithExternalHttpEndpoints();

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
