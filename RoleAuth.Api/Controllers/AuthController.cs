using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleAuth.Api.Dtos;
using RoleAuth.Api.Services;

namespace RoleAuth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Реєстрація. Повертає JWT токен.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Логін. Повертає JWT токен з ролями користувача.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        Ok(await authService.LoginAsync(request, ct));

    /// <summary>Показує, що саме прочитано з токена (для перевірки ролей).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        Id = User.FindFirst("sub")?.Value,
        Username = User.Identity?.Name,
        Roles = User.FindAll(JwtService.RoleClaim).Select(c => c.Value).ToList()
    });
}
