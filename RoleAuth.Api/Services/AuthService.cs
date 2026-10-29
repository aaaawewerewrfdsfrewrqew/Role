using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Dtos;
using RoleAuth.Api.Exceptions;
using RoleAuth.Api.Models;
using RoleAuth.Api.Repositories;

namespace RoleAuth.Api.Services;

public class AuthService(
    IUserRepository users,
    IRoleRepository roles,
    IPasswordHasher<User> passwordHasher,
    IJwtService jwtService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await users.ExistsByUsernameAsync(username, ct))
            throw new ConflictException("Ім'я користувача вже зайняте");

        if (await users.ExistsByEmailAsync(email, ct))
            throw new ConflictException("Email вже використовується");

        var defaultRole = await roles.GetByNameAsync(RoleNames.User, ct)
                          ?? throw new InvalidOperationException($"Роль '{RoleNames.User}' не знайдено в БД");

        var user = new User
        {
            Username = username,
            Email = email,
            BirthDate = request.BirthDate,
            IsSubscribed = request.IsSubscribed
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.Roles.Add(defaultRole);

        await users.AddAsync(user, ct);

        try
        {
            await users.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Користувач з такими даними вже існує");
        }

        // Після успішної реєстрації повертаємо JWT
        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByUsernameAsync(request.Username.Trim(), ct);

        var verification = user is null
            ? PasswordVerificationResult.Failed
            : passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (user is null || verification == PasswordVerificationResult.Failed)
            throw new UnauthorizedAppException("Невірний логін або пароль");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = jwtService.GenerateToken(user);
        return new AuthResponse(token, expiresAt, user.Username, user.Roles.Select(r => r.Name).ToList());
    }
}
