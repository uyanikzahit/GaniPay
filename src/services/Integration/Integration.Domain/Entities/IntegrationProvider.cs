namespace GaniPay.Integration.Domain.Entities;

public sealed class IntegrationProvider
{
    public Guid Id { get; set; }

    public string Code { get; set; } = default!;     // MOCK_BANK, PSP_X
    public string Name { get; set; } = default!;     // Provider adý
    public string Type { get; set; } = default!;     // payment / notification / other
    public string BaseUrl { get; set; } = default!;  // MVP'de mock olabilir

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}