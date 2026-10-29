namespace RoleAuth.Api.Models;

public enum UserTokenType
{
    PasswordReset = 1,
    EmailChange = 2
}

/// <summary>
/// Одноразовий токен (скидання пароля / підтвердження зміни email).
/// У БД зберігається лише SHA-256 хеш токена; сам токен існує тільки в листі.
/// </summary>
public class UserToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public UserTokenType Type { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    /// <summary>Новий email (лише для EmailChange).</summary>
    public string? NewEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
