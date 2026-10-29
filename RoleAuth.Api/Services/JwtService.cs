using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RoleAuth.Api.Models;
using RoleAuth.Api.Options;

namespace RoleAuth.Api.Services;

public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    /// <summary>Назва claim з ролями (на неї налаштований RoleClaimType у Program.cs).</summary>
    public const string RoleClaim = "role";

    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Ролі користувача додаються до токена
        claims.AddRange(user.Roles.Select(r => new Claim(RoleClaim, r.Name)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
