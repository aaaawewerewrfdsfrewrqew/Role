using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Dtos;
using RoleAuth.Api.Exceptions;
using RoleAuth.Api.Models;
using RoleAuth.Api.Repositories;

namespace RoleAuth.Api.Services;

public class RoleService(IRoleRepository roles) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default) =>
        (await roles.GetAllAsync(ct)).Select(ToDto).ToList();

    public async Task<RoleDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var role = await roles.GetByIdAsync(id, ct) ?? throw new NotFoundException("Роль не знайдено");
        return ToDto(role);
    }

    public async Task<RoleDto> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var normalized = Normalize(name);
        var role = await roles.GetByNameAsync(normalized, ct)
                   ?? throw new NotFoundException($"Роль '{normalized}' не знайдено");
        return ToDto(role);
    }

    public async Task<RoleDto> CreateAsync(string name, CancellationToken ct = default)
    {
        var normalized = Normalize(name);

        if (await roles.ExistsByNameAsync(normalized, null, ct))
            throw new ConflictException($"Роль '{normalized}' вже існує");

        var role = new Role { Name = normalized };
        await roles.AddAsync(role, ct);
        await SaveAsync(normalized, ct);
        return ToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        var role = await roles.GetByIdAsync(id, ct) ?? throw new NotFoundException("Роль не знайдено");
        var normalized = Normalize(name);

        if (role.Name == normalized)
            return ToDto(role);

        if (RoleNames.System.Contains(role.Name))
            throw new BadRequestException($"Системну роль '{role.Name}' не можна перейменувати");

        if (await roles.ExistsByNameAsync(normalized, id, ct))
            throw new ConflictException($"Роль '{normalized}' вже існує");

        role.Name = normalized;
        await SaveAsync(normalized, ct);
        return ToDto(role);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var role = await roles.GetByIdAsync(id, ct) ?? throw new NotFoundException("Роль не знайдено");

        if (RoleNames.System.Contains(role.Name))
            throw new BadRequestException($"Системну роль '{role.Name}' не можна видалити");

        roles.Remove(role);
        await roles.SaveChangesAsync(ct);
    }

    // Захист від гонок: унікальний індекс у БД спрацює, навіть якщо перевірка вище пройшла.
    private async Task SaveAsync(string name, CancellationToken ct)
    {
        try
        {
            await roles.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException($"Роль '{name}' вже існує");
        }
    }

    private static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Ім'я ролі не може бути порожнім");
        return name.Trim().ToUpperInvariant();
    }

    private static RoleDto ToDto(Role r) => new(r.Id, r.Name);
}
