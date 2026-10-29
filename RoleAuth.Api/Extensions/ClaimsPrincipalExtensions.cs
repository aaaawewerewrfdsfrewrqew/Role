using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RoleAuth.Api.Exceptions;

namespace RoleAuth.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(value, out var id) ? id : throw new UnauthorizedAppException("Недійсний токен");
    }
}
