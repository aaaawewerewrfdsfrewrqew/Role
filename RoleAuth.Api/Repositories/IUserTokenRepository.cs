using RoleAuth.Api.Models;

namespace RoleAuth.Api.Repositories;

public interface IUserTokenRepository
{
    /// <summary>Повертає токен (разом з User), якщо він існує, потрібного типу і не прострочений.</summary>
    Task<UserToken?> GetValidAsync(string tokenHash, UserTokenType type, CancellationToken ct = default);

    Task AddAsync(UserToken token, CancellationToken ct = default);
    void Remove(UserToken token);

    /// <summary>Позначає на видалення всі токени користувача цього типу (крім exceptId).</summary>
    Task RemoveAllAsync(int userId, UserTokenType type, int? exceptId = null, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
