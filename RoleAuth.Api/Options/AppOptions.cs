namespace RoleAuth.Api.Options;

public class AppOptions
{
    public const string SectionName = "App";

    /// <summary>Базова адреса, яка підставляється в посилання з листів.</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5080";
    public int PasswordResetTokenMinutes { get; set; } = 60;
    public int EmailChangeTokenMinutes { get; set; } = 1440;
}
