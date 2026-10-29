using Microsoft.Extensions.Options;
using RoleAuth.Api.Options;

namespace RoleAuth.Api.Services;

/// <summary>
/// Фоновий сервіс: кожні N хвилин перевіряє, чи не настав час привітати іменинників.
/// Дублікати виключає поле User.LastBirthdayGreetingYear, тому часті перевірки безпечні,
/// а якщо застосунок був вимкнений зранку — привітання піде при наступному запуску того ж дня.
/// </summary>
public class BirthdayGreetingWorker(
    IServiceScopeFactory scopeFactory,
    BirthdayClock clock,
    IOptions<BirthdayOptions> options,
    ILogger<BirthdayGreetingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        if (!settings.Enabled)
        {
            logger.LogInformation("Привітання з днем народження вимкнено (Birthday:Enabled = false)");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, settings.CheckIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                if (clock.LocalNow.Hour >= settings.SendAtHour)
                {
                    using var scope = scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IBirthdayService>();
                    var sent = await service.SendDueGreetingsAsync(stoppingToken);

                    if (sent > 0)
                        logger.LogInformation("Надіслано привітань з днем народження: {Count}", sent);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Помилка під час розсилки привітань");
            }
        }
        while (await NextTickAsync(timer, stoppingToken));
    }

    private static async Task<bool> NextTickAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
