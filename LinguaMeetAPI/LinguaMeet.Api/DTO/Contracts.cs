using System.ComponentModel.DataAnnotations;

namespace LinguaMeet.Api.DTO;

public record RegisterRequestDto(
    [Required, MaxLength(80)] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string PreferredLanguage
);

public record LoginRequestDto([Required, EmailAddress] string Email, [Required] string Password);

public record LoginResponseDto(
    string Token,
    int UserId,
    string DisplayName,
    string Email,
    string PreferredLanguage
);

public record UserProfileDto(int Id, string DisplayName, string Email, string PreferredLanguage);

public record JoinMeetingDto([Required] string SelectedLanguage);

public record MeetingDto(
    int Id,
    string RoomCode,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? EndedAt,
    int ParticipantCount
);

public record SaveTranscriptDto([Required] string Text, [Required] string SourceLanguage);

public record TranscriptDto(
    int Id,
    int SpeakerId,
    string SpeakerName,
    string OriginalText,
    string SourceLanguage,
    DateTime CreatedAt
);
