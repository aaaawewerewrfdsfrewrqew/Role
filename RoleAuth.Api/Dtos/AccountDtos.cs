using System.ComponentModel.DataAnnotations;

namespace RoleAuth.Api.Dtos;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangeEmailRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string NewEmail { get; set; } = string.Empty;

    /// <summary>Поточний пароль — підтвердження, що змінює пошту саме власник акаунта.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ConfirmEmailChangeRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public record MessageResponse(string Message);

public record ProfileResponse(string Username, string Email, DateOnly? BirthDate, bool IsSubscribed);

public class UpdateProfileRequest
{
    /// <summary>Дата народження (yyyy-MM-dd). null — прибрати дату.</summary>
    [BirthDate]
    public DateOnly? BirthDate { get; set; }

    /// <summary>true — отримувати привітання, false — відписатися.</summary>
    public bool IsSubscribed { get; set; }
}
