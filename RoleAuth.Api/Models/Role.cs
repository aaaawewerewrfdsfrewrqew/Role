namespace RoleAuth.Api.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}

public static class RoleNames
{
    public const string User = "USER";
    public const string Admin = "ADMIN";

    /// <summary>Системні ролі: їх не можна видалити або перейменувати.</summary>
    public static readonly string[] System = [User, Admin];
}
