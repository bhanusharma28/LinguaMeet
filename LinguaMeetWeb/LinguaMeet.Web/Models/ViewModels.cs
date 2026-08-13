using System.ComponentModel.DataAnnotations;

namespace LinguaMeet.Web.Models;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class RegisterViewModel
{
    [Required]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    public string PreferredLanguage { get; set; } = "en";
}

public record LoginResult(
    string Token,
    int UserId,
    string DisplayName,
    string Email,
    string PreferredLanguage
);

public record MeetingItem(
    int Id,
    string RoomCode,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? EndedAt,
    int ParticipantCount
);

public record TranscriptItem(
    int Id,
    int SpeakerId,
    string SpeakerName,
    string OriginalText,
    string SourceLanguage,
    DateTime CreatedAt
);

public class DashboardViewModel
{
    public string Name { get; set; } = "";
    public List<MeetingItem> Meetings { get; set; } = [];
}

public class MeetingViewModel
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = "";
    public string Language { get; set; } = "en";
    public string Token { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public static class LanguageOptions
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "en", "English" },
        { "hi", "Hindi" },
        { "es", "Spanish" },
        { "fr", "French" },
        { "de", "German" },
        { "ar", "Arabic" },
        { "zh", "Chinese" },
        { "ja", "Japanese" },
        { "pt", "Portuguese" },
        { "bn", "Bengali" },
        { "mr", "Marathi" },
        { "gu", "Gujarati" },
        { "pa", "Punjabi" },
        { "ta", "Tamil" },
        { "te", "Telugu" },
        { "kn", "Kannada" },
        { "ml", "Malayalam" },
        { "ur", "Urdu" },
        { "ru", "Russian" },
        { "it", "Italian" },
        { "ko", "Korean" },
        { "nl", "Dutch" },
        { "tr", "Turkish" },
        { "pl", "Polish" },
        { "id", "Indonesian" },
        { "vi", "Vietnamese" },
        { "th", "Thai" },
        { "ne", "Nepali" },
    };
}
