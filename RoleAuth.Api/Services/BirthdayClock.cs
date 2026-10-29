using Microsoft.Extensions.Options;
using RoleAuth.Api.Options;

namespace RoleAuth.Api.Services;

/// <summary>Поточний локальний час у часовому поясі з налаштувань (для визначення "сьогодні").</summary>
public class BirthdayClock
{
    private readonly TimeZoneInfo _timeZone;

    public BirthdayClock(IOptions<BirthdayOptions> options, ILogger<BirthdayClock> logger)
    {
        var id = options.Value.TimeZone;
        try
        {
            _timeZone = string.IsNullOrWhiteSpace(id)
                ? TimeZoneInfo.Local
                : TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            logger.LogWarning("Часовий пояс '{TimeZone}' не знайдено, використовується локальний ({Local})",
                id, TimeZoneInfo.Local.Id);
            _timeZone = TimeZoneInfo.Local;
        }
    }

    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
}
