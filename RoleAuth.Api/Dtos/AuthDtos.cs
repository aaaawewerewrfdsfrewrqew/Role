using System.ComponentModel.DataAnnotations;

namespace RoleAuth.Api.Dtos;

public class RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Дата народження, формат yyyy-MM-dd (необов'язково).</summary>
    [BirthDate]
    public DateOnly? BirthDate { get; set; }

    /// <summary>Підписатися на привітання та розсилки.</summary>
    public bool IsSubscribed { get; set; }
}

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, DateTime ExpiresAt, string Username, IReadOnlyList<string> Roles);
