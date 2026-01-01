namespace GaniPay.Login.Worker.Models;

public sealed class IdentityLoginResponse
{
    public bool Success { get; set; }
    public string? CustomerId { get; set; }
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }

    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
}