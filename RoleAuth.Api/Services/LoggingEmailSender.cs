using System.Net;
using System.Text.RegularExpressions;

namespace RoleAuth.Api.Services;

/// <summary>Для розробки: лист не відправляється, а виводиться в лог (вікно Output / консоль).</summary>
public partial class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var link = HrefRegex().Match(htmlBody) is { Success: true } m
            ? WebUtility.HtmlDecode(m.Groups[1].Value)
            : "(немає)";

        logger.LogWarning(
            "[DEV EMAIL] Лист НЕ відправлено, лише записано в лог.\n  Кому: {To}\n  Тема: {Subject}\n  Посилання: {Link}",
            to, subject, link);

        return Task.CompletedTask;
    }

    [GeneratedRegex("href=\"([^\"]+)\"")]
    private static partial Regex HrefRegex();
}
