using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleAuth.Api.Dtos;
using RoleAuth.Api.Extensions;
using RoleAuth.Api.Services;

namespace RoleAuth.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController(IAccountService account) : ControllerBase
{
    /// <summary>Профіль поточного користувача.</summary>
    [HttpGet("profile")]
    public async Task<ActionResult<ProfileResponse>> GetProfile(CancellationToken ct) =>
        Ok(await account.GetProfileAsync(User.GetUserId(), ct));

    /// <summary>Зміна дати народження та підписки на привітання.</summary>
    [HttpPut("profile")]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile(
        [FromBody] UpdateProfileRequest request, CancellationToken ct) =>
        Ok(await account.UpdateProfileAsync(User.GetUserId(), request, ct));

    /// <summary>Зміна пароля авторизованим користувачем (потрібен поточний пароль).</summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<MessageResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await account.ChangePasswordAsync(User.GetUserId(), request, ct);
        return Ok(new MessageResponse("Пароль змінено"));
    }

    /// <summary>Запит на скидання пароля: на пошту надсилається лист із посиланням.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await account.ForgotPasswordAsync(request, ct);
        // Відповідь однакова незалежно від того, чи існує email
        return Ok(new MessageResponse("Якщо акаунт з такою поштою існує, ми надіслали на неї лист з інструкцією"));
    }

    /// <summary>Встановлення нового пароля за токеном з листа.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await account.ResetPasswordAsync(request, ct);
        return Ok(new MessageResponse("Пароль успішно змінено. Тепер можна увійти з новим паролем"));
    }

    /// <summary>Запит на зміну email: лист із підтвердженням надсилається на нову адресу.</summary>
    [HttpPost("change-email")]
    public async Task<ActionResult<MessageResponse>> ChangeEmail(
        [FromBody] ChangeEmailRequest request, CancellationToken ct)
    {
        await account.RequestEmailChangeAsync(User.GetUserId(), request, ct);
        return Ok(new MessageResponse($"Ми надіслали лист із підтвердженням на {request.NewEmail.Trim()}. Email зміниться після підтвердження"));
    }

    /// <summary>Підтвердження зміни email за токеном з листа.</summary>
    [HttpPost("confirm-email-change")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ConfirmEmailChange(
        [FromBody] ConfirmEmailChangeRequest request, CancellationToken ct)
    {
        await account.ConfirmEmailChangeAsync(request, ct);
        return Ok(new MessageResponse("Email успішно змінено"));
    }
}
