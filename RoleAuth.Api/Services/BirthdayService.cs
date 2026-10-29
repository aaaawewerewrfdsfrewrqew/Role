using RoleAuth.Api.Repositories;

namespace RoleAuth.Api.Services;

public class BirthdayService(
    IUserRepository users,
    IEmailSender emailSender,
    BirthdayClock clock,
    ILogger<BirthdayService> logger) : IBirthdayService
{
    public Task<int> SendDueGreetingsAsync(CancellationToken ct = default) =>
        SendDueGreetingsAsync(DateOnly.FromDateTime(clock.LocalNow), ct);

    public async Task<int> SendDueGreetingsAsync(DateOnly today, CancellationToken ct = default)
    {
        // Підписані, з датою народження і ще не привітані цього року
        var candidates = await users.GetBirthdayCandidatesAsync(today.Year, ct);

        var ids = candidates
            .Where(c => IsBirthday(c.BirthDate, today))
            .Select(c => c.Id)
            .ToList();

        if (ids.Count == 0)
            return 0;

        var sent = 0;
        foreach (var user in await users.GetByIdsAsync(ids, ct))
        {
            try
            {
                await emailSender.SendAsync(user.Email, "З днем народження! 🎉",
                    EmailTemplates.BirthdayGreeting(user.Username), ct);

                // Зберігаємо одразу для кожного, щоб збій на наступному не призвів до повторної відправки
                user.LastBirthdayGreetingYear = today.Year;
                await users.SaveChangesAsync(ct);
                sent++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Рік не позначено, тому спроба повториться на наступному циклі
                logger.LogError(ex, "Не вдалося привітати користувача {UserId}", user.Id);
            }
        }

        return sent;
    }

    /// <summary>Народжені 29 лютого у невисокосні роки вітаються 28 лютого.</summary>
    internal static bool IsBirthday(DateOnly birth, DateOnly today)
    {
        if (birth.Month == 2 && birth.Day == 29 && !DateTime.IsLeapYear(today.Year))
            return today.Month == 2 && today.Day == 28;

        return birth.Month == today.Month && birth.Day == today.Day;
    }
}
