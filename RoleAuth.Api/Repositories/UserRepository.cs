using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Data;
using RoleAuth.Api.Models;

namespace RoleAuth.Api.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<List<BirthdayCandidate>> GetBirthdayCandidatesAsync(int year, CancellationToken ct = default) =>
        db.Users
            .AsNoTracking()
            .Where(u => u.IsSubscribed
                        && u.BirthDate != null
                        && (u.LastBirthdayGreetingYear == null || u.LastBirthdayGreetingYear != year))
            .Select(u => new BirthdayCandidate(u.Id, u.BirthDate!.Value))
            .ToListAsync(ct);

    public Task<List<User>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var list = ids.ToList();
        return db.Users.Where(u => list.Contains(u.Id)).ToListAsync(ct);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
