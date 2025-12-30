using System.Text.Json;

namespace GaniPay.Validation.Worker.Handlers;

public sealed class JobVariables
{
    private readonly JsonElement _root;

    public JobVariables(JsonElement root) => _root = root;

    public bool TryGetString(string name, out string? value)
    {
        value = null;
        if (_root.ValueKind != JsonValueKind.Object) return false;
        if (!_root.TryGetProperty(name, out var el)) return false;

        value = el.ValueKind == JsonValueKind.String ? el.GetString() : el.ToString();
        return true;
    }

    public bool TryGetGuid(string name, out Guid value)
    {
        value = default;
        return TryGetString(name, out var s) && Guid.TryParse(s, out value);
    }

    public bool TryGetInt(string name, out int value)
    {
        value = default;
        if (_root.ValueKind != JsonValueKind.Object) return false;
        if (!_root.TryGetProperty(name, out var el)) return false;

        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out value)) return true;
        if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out value)) return true;

        return false;
    }
}
