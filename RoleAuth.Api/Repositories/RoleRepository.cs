using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Data;
using RoleAuth.Api.Models;

namespace RoleAuth.Api.Repositories;

public class RoleRepository(AppDbContext db) : IRoleRepository
{
    public Task<List<Role>> GetAllAsync(CancellationToken ct = default) =>
        db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(ct);

    public Task<Role?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default) =>
        db.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default) =>
        db.Roles.AnyAsync(r => r.Name == name && (excludeId == null || r.Id != excludeId), ct);

    public async Task AddAsync(Role role, CancellationToken ct = default) =>
        await db.Roles.AddAsync(role, ct);

    public void Remove(Role role) => db.Roles.Remove(role);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
