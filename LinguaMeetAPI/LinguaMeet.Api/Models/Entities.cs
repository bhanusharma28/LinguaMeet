using System.ComponentModel.DataAnnotations;

namespace LinguaMeet.Api.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string DisplayName { get; set; } = "";

    [MaxLength(160)]
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "en";
    public string? ProfileImage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<MeetingParticipant> Participations { get; set; } = [];
}

public class Meeting
{
    public int Id { get; set; }

    [MaxLength(6)]
    public string RoomCode { get; set; } = "";
    public int CreatedBy { get; set; }
    public User Creator { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<MeetingParticipant> Participants { get; set; } = [];
    public List<TranscriptSegment> TranscriptSegments { get; set; } = [];
}

public class MeetingParticipant
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(10)]
    public string SelectedLanguage { get; set; } = "en";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
}

public class TranscriptSegment
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public int SpeakerId { get; set; }
    public User Speaker { get; set; } = null!;
    public string OriginalText { get; set; } = "";

    [MaxLength(10)]
    public string SourceLanguage { get; set; } = "en";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
