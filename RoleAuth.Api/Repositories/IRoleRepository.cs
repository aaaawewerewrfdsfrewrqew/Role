using RoleAuth.Api.Models;

namespace RoleAuth.Api.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    void Remove(Role role);
    Task SaveChangesAsync(CancellationToken ct = default);
}
