using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoleAuth.Api.Dtos;
using RoleAuth.Api.Exceptions;
using RoleAuth.Api.Models;
using RoleAuth.Api.Options;
using RoleAuth.Api.Repositories;

namespace RoleAuth.Api.Services;

public class AccountService(
    IUserRepository users,
    IUserTokenRepository tokens,
    IPasswordHasher<User> passwordHasher,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions,
    ILogger<AccountService> logger) : IAccountService
{
    private const string InvalidLinkMessage = "Посилання недійсне або термін його дії минув";

    private readonly AppOptions _app = appOptions.Value;

    // ---------------------------------------------------------------- 1. Зміна пароля

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        EnsurePasswordMatches(user, request.CurrentPassword);

        if (request.CurrentPassword == request.NewPassword)
            throw new BadRequestException("Новий пароль має відрізнятися від поточного");

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        await users.SaveChangesAsync(ct);

        await TrySendAsync(user.Email, "Пароль змінено", EmailTemplates.PasswordChanged(user.Username), ct);
    }

    // ---------------------------------------------------------------- 2. Скидання пароля

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim(), ct);

        // Не розкриваємо, чи існує такий email: поведінка назовні однакова.
        if (user is null)
            return;

        await tokens.RemoveAllAsync(user.Id, UserTokenType.PasswordReset, null, ct);

        var (raw, hash) = SecureToken.Generate();
        var lifetime = TimeSpan.FromMinutes(_app.PasswordResetTokenMinutes);

        await tokens.AddAsync(new UserToken
        {
            UserId = user.Id,
            Type = UserTokenType.PasswordReset,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        }, ct);
        await tokens.SaveChangesAsync(ct);

        var link = BuildLink("reset-password.html", raw);
        await TrySendAsync(user.Email, "Скидання пароля",
            EmailTemplates.ResetPassword(user.Username, link, lifetime), ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var token = await tokens.GetValidAsync(SecureToken.Hash(request.Token.Trim()), UserTokenType.PasswordReset, ct)
                    ?? throw new BadRequestException(InvalidLinkMessage);

        var user = token.User;
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);

        // Токен одноразовий; решту невикористаних токенів скидання теж прибираємо
        tokens.Remove(token);
        await tokens.RemoveAllAsync(user.Id, UserTokenType.PasswordReset, token.Id, ct);
        await tokens.SaveChangesAsync(ct);

        await TrySendAsync(user.Email, "Пароль змінено", EmailTemplates.PasswordChanged(user.Username), ct);
    }

    // ---------------------------------------------------------------- 3. Зміна email

    public async Task RequestEmailChangeAsync(int userId, ChangeEmailRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        EnsurePasswordMatches(user, request.Password);

        var newEmail = request.NewEmail.Trim();

        if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Це вже ваш поточний email");

        if (await users.ExistsByEmailAsync(newEmail, ct))
            throw new ConflictException("Email вже використовується");

        await tokens.RemoveAllAsync(user.Id, UserTokenType.EmailChange, null, ct);

        var (raw, hash) = SecureToken.Generate();
        var lifetime = TimeSpan.FromMinutes(_app.EmailChangeTokenMinutes);

        await tokens.AddAsync(new UserToken
        {
            UserId = user.Id,
            Type = UserTokenType.EmailChange,
            TokenHash = hash,
            NewEmail = newEmail,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        }, ct);
        await tokens.SaveChangesAsync(ct);

        // Лист із підтвердженням іде на НОВУ адресу. Email у акаунті не змінюється, доки її не підтвердять.
        var link = BuildLink("confirm-email.html", raw);
        try
        {
            await emailSender.SendAsync(newEmail, "Підтвердіть новий email",
                EmailTemplates.ConfirmEmailChange(user.Username, link, lifetime), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не вдалося надіслати лист підтвердження на {Email}", newEmail);
            throw new EmailSendException();
        }
    }

    public async Task ConfirmEmailChangeAsync(ConfirmEmailChangeRequest request, CancellationToken ct = default)
    {
        var token = await tokens.GetValidAsync(SecureToken.Hash(request.Token.Trim()), UserTokenType.EmailChange, ct)
                    ?? throw new BadRequestException(InvalidLinkMessage);

        var newEmail = token.NewEmail ?? throw new BadRequestException(InvalidLinkMessage);

        // Поки лист «лежав», цю адресу міг зайняти хтось інший
        if (await users.ExistsByEmailAsync(newEmail, ct))
            throw new ConflictException("Email вже використовується");

        var user = token.User;
        var oldEmail = user.Email;
        user.Email = newEmail;

        tokens.Remove(token);
        await tokens.RemoveAllAsync(user.Id, UserTokenType.EmailChange, token.Id, ct);

        try
        {
            await tokens.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Email вже використовується");
        }

        // Повідомлення на стару адресу — щоб власник побачив, якщо зміна була не його
        await TrySendAsync(oldEmail, "Email змінено", EmailTemplates.EmailChanged(user.Username, newEmail), ct);
    }

    // ---------------------------------------------------------------- Профіль

    public async Task<ProfileResponse> GetProfileAsync(int userId, CancellationToken ct = default) =>
        ToProfile(await GetUserAsync(userId, ct));

    public async Task<ProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        user.BirthDate = request.BirthDate;
        user.IsSubscribed = request.IsSubscribed;
        await users.SaveChangesAsync(ct);
        return ToProfile(user);
    }

    private static ProfileResponse ToProfile(User u) => new(u.Username, u.Email, u.BirthDate, u.IsSubscribed);

    // ---------------------------------------------------------------- helpers

    private async Task<User> GetUserAsync(int userId, CancellationToken ct) =>
        await users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("Користувача не знайдено");

    private void EnsurePasswordMatches(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            throw new BadRequestException("Невірний поточний пароль");
    }

    private string BuildLink(string page, string rawToken) =>
        $"{_app.PublicBaseUrl.TrimEnd('/')}/{page}?token={Uri.EscapeDataString(rawToken)}";

    /// <summary>Для необов'язкових листів: помилка відправки не має ламати основну операцію.</summary>
    private async Task TrySendAsync(string to, string subject, string html, CancellationToken ct)
    {
        try
        {
            await emailSender.SendAsync(to, subject, html, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не вдалося надіслати лист '{Subject}' на {Email}", subject, to);
        }
    }
}
