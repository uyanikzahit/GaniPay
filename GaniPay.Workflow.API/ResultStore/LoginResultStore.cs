using System.Collections.Concurrent;

namespace GaniPay.Workflow.API.ResultStore;

public static class LoginResultStore
{
    // correlationId -> (result, expiresAtUtc)
    private static readonly ConcurrentDictionary<string, (LoginResult Result, DateTime ExpiresAtUtc)> _results = new();

    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public static void Set(string correlationId, LoginResult result, TimeSpan? ttl = null)
    {
        var expires = DateTime.UtcNow.Add(ttl ?? DefaultTtl);
        _results[correlationId] = (result, expires);
    }

    public static bool TryGet(string correlationId, out LoginResult result)
    {
        result = default!;

        if (!_results.TryGetValue(correlationId, out var entry))
            return false;

        if (DateTime.UtcNow > entry.ExpiresAtUtc)
        {
            _results.TryRemove(correlationId, out _);
            return false;
        }

        result = entry.Result;
        return true;
    }

    public static void Remove(string correlationId)
    {
        _results.TryRemove(correlationId, out _);
    }
}