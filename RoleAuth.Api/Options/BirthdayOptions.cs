namespace RoleAuth.Api.Options;

public class BirthdayOptions
{
    public const string SectionName = "Birthday";

    public bool Enabled { get; set; } = true;

    /// <summary>Часовий пояс, у якому визначається "сьогодні" (IANA, напр. Europe/Kyiv).</summary>
    public string TimeZone { get; set; } = "Europe/Kyiv";

    /// <summary>Не раніше цієї години (за TimeZone) надсилати привітання.</summary>
    public int SendAtHour { get; set; } = 9;

    public int CheckIntervalMinutes { get; set; } = 30;
}
