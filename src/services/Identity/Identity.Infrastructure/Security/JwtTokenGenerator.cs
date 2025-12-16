using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GaniPay.Identity.Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GaniPay.Identity.Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
        => _options = options.Value;

    public string GenerateToken(Guid customerId, string? phone, string role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, customerId.ToString()),
            new("customer_id", customerId.ToString()),
            new(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrWhiteSpace(phone))
            claims.Add(new(JwtRegisteredClaimNames.PhoneNumber, phone));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
