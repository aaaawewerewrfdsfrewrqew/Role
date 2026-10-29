namespace RoleAuth.Api.Services;

public interface IBirthdayService
{
    /// <summary>Надсилає привітання всім підписаним іменинникам "сьогодні". Повертає кількість надісланих.</summary>
    Task<int> SendDueGreetingsAsync(CancellationToken ct = default);

    Task<int> SendDueGreetingsAsync(DateOnly today, CancellationToken ct = default);
}
