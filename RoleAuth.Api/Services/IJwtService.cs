using RoleAuth.Api.Models;

namespace RoleAuth.Api.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
