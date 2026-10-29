namespace RoleAuth.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Дата народження (необов'язкова).</summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>Чи підписаний користувач на привітання/розсилки.</summary>
    public bool IsSubscribed { get; set; }

    /// <summary>Рік, у якому вже надіслано привітання (щоб не надсилати двічі).</summary>
    public int? LastBirthdayGreetingYear { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
