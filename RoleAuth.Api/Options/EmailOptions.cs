namespace RoleAuth.Api.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>"Log" — не відправляти, а писати лист у лог; "Smtp" — реально відправляти.</summary>
    public string Provider { get; set; } = "Log";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "RoleAuth";
}
