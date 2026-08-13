using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LinguaMeet.Api.Helpers;

public static class Languages
{
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        ["en"] = "English",
        ["hi"] = "Hindi",
        ["es"] = "Spanish",
        ["fr"] = "French",
        ["de"] = "German",
        ["ar"] = "Arabic",
        ["zh"] = "Chinese",
        ["ja"] = "Japanese",
        ["pt"] = "Portuguese",
        ["bn"] = "Bengali",
        ["mr"] = "Marathi",
        ["gu"] = "Gujarati",
        ["pa"] = "Punjabi",
        ["ta"] = "Tamil",
        ["te"] = "Telugu",
        ["kn"] = "Kannada",
        ["ml"] = "Malayalam",
        ["ur"] = "Urdu",
        ["ru"] = "Russian",
        ["it"] = "Italian",
        ["ko"] = "Korean",
        ["nl"] = "Dutch",
        ["tr"] = "Turkish",
        ["pl"] = "Polish",
        ["id"] = "Indonesian",
        ["vi"] = "Vietnamese",
        ["th"] = "Thai",
        ["ne"] = "Nepali",
    };

    public static bool IsSupported(string code) => All.ContainsKey(code);
}

public static class RoomCodeGenerator
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        Span<char> result = stackalloc char[6];
        for (var i = 0; i < result.Length; i++)
            result[i] = Chars[RandomNumberGenerator.GetInt32(Chars.Length)];
        return new string(result);
    }
}

public sealed class JwtHelper(IConfiguration configuration)
{
    public string Create(int id, string name, string email)
    {
        var key =
            configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            )
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
