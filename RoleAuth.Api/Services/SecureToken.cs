using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace RoleAuth.Api.Services;

public static class SecureToken
{
    /// <summary>Raw — те, що йде в лист; Hash — те, що зберігається в БД.</summary>
    public static (string Raw, string Hash) Generate()
    {
        var raw = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        return (raw, Hash(raw));
    }

    public static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
