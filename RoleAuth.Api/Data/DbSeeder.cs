using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Models;

namespace RoleAuth.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await db.Database.EnsureCreatedAsync();

        // Системні ролі
        foreach (var name in RoleNames.System)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Name = name });
        }
        await db.SaveChangesAsync();

        // Початковий адміністратор (щоб можна було керувати ролями)
        var username = config["SeedAdmin:Username"];
        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
            return;

        if (await db.Users.AnyAsync(u => u.Username == username))
            return;

        var admin = new User { Username = username, Email = email };
        admin.PasswordHash = hasher.HashPassword(admin, password);
        admin.Roles = await db.Roles
            .Where(r => r.Name == RoleNames.Admin || r.Name == RoleNames.User)
            .ToListAsync();

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
