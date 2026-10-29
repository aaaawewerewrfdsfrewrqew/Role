using System.Net;

namespace RoleAuth.Api.Services;

public static class EmailTemplates
{
    public static string ResetPassword(string username, string link, TimeSpan lifetime) =>
        Layout("Скидання пароля",
            $"Привіт, {Enc(username)}! Ми отримали запит на скидання пароля до вашого акаунта.",
            "Змінити пароль", link,
            $"Посилання дійсне {Format(lifetime)}. Якщо ви не робили цього запиту, просто проігноруйте лист.");

    public static string ConfirmEmailChange(string username, string link, TimeSpan lifetime) =>
        Layout("Підтвердження нового email",
            $"Привіт, {Enc(username)}! Підтвердіть, що ця адреса має стати новою поштою вашого акаунта.",
            "Підтвердити email", link,
            $"Посилання дійсне {Format(lifetime)}. Якщо ви не робили цього запиту, проігноруйте лист — пошта не зміниться.");

    public static string PasswordChanged(string username) =>
        Layout("Пароль змінено",
            $"Привіт, {Enc(username)}! Пароль до вашого акаунта щойно було змінено.",
            null, null,
            "Якщо це були не ви, негайно скиньте пароль через «Забули пароль?» та зверніться до підтримки.");

    public static string EmailChanged(string username, string newEmail) =>
        Layout("Email змінено",
            $"Привіт, {Enc(username)}! Пошту вашого акаунта змінено на {Enc(newEmail)}.",
            null, null,
            "Якщо це були не ви, негайно зверніться до підтримки.");

    public static string BirthdayGreeting(string username) =>
        Layout("З днем народження! 🎉",
            $"Вітаємо, {Enc(username)}! Бажаємо здоров'я, радості та здійснення всіх планів. Гарного вам дня!",
            null, null,
            "Ви отримали цей лист, бо підписані на привітання. Відписатися можна в налаштуваннях профілю.");

    private static string Layout(string title, string introHtml, string? buttonText, string? url, string footer)
    {
        var button = url is null
            ? ""
            : $"<p style=\"margin:24px 0\"><a href=\"{Enc(url)}\" style=\"background:#2563eb;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;display:inline-block\">{Enc(buttonText ?? "Відкрити")}</a></p>" +
              $"<p style=\"font-size:12px;color:#666\">Якщо кнопка не працює, скопіюйте посилання в браузер:<br>{Enc(url)}</p>";

        return $"<div style=\"font-family:Arial,sans-serif;max-width:480px;margin:0 auto\"><h2>{Enc(title)}</h2><p>{introHtml}</p>{button}<p style=\"font-size:12px;color:#666\">{Enc(footer)}</p></div>";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s);

    private static string Format(TimeSpan t) =>
        t.TotalHours >= 1 && t.TotalMinutes % 60 == 0 ? $"{(int)t.TotalHours} год." : $"{(int)t.TotalMinutes} хв.";
}
