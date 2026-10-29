using RoleAuth.Api.Dtos;

namespace RoleAuth.Api.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoleDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RoleDto> GetByNameAsync(string name, CancellationToken ct = default);
    Task<RoleDto> CreateAsync(string name, CancellationToken ct = default);
    Task<RoleDto> UpdateAsync(int id, string name, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
