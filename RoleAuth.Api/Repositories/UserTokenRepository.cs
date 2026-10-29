using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Data;
using RoleAuth.Api.Models;

namespace RoleAuth.Api.Repositories;

public class UserTokenRepository(AppDbContext db) : IUserTokenRepository
{
    public Task<UserToken?> GetValidAsync(string tokenHash, UserTokenType type, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return db.UserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.Type == type && t.ExpiresAt > now, ct);
    }

    public async Task AddAsync(UserToken token, CancellationToken ct = default) =>
        await db.UserTokens.AddAsync(token, ct);

    public void Remove(UserToken token) => db.UserTokens.Remove(token);

    public async Task RemoveAllAsync(int userId, UserTokenType type, int? exceptId = null, CancellationToken ct = default)
    {
        var items = await db.UserTokens
            .Where(t => t.UserId == userId && t.Type == type && (exceptId == null || t.Id != exceptId))
            .ToListAsync(ct);
        db.UserTokens.RemoveRange(items);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
