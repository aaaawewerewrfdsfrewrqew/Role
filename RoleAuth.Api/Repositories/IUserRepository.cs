using RoleAuth.Api.Models;

namespace RoleAuth.Api.Repositories;

/// <summary>Мінімальні дані для вибору іменинників.</summary>
public record BirthdayCandidate(int Id, DateOnly BirthDate);

public interface IUserRepository
{
    Task<List<BirthdayCandidate>> GetBirthdayCandidatesAsync(int year, CancellationToken ct = default);
    Task<List<User>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
