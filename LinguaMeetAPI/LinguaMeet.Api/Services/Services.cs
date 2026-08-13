using LinguaMeet.Api.DTO;
using LinguaMeet.Api.Helpers;
using LinguaMeet.Api.Interfaces.Repositories;
using LinguaMeet.Api.Interfaces.Services;
using LinguaMeet.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace LinguaMeet.Api.Services;

public class AuthService(IUserRepository users, JwtHelper jwt, ILogger<AuthService> log)
    : IAuthService
{
    private readonly PasswordHasher<User> hasher = new();

    public async Task RegisterAsync(RegisterRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await users.ByEmailAsync(email) != null)
            throw new InvalidOperationException("An account with this email already exists.");
        if (!Languages.IsSupported(dto.PreferredLanguage))
            throw new InvalidOperationException("Unsupported language.");
        var user = new User
        {
            DisplayName = dto.DisplayName.Trim(),
            Email = email,
            PreferredLanguage = dto.PreferredLanguage,
        };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        await users.AddAsync(user);
        await users.SaveAsync();
        log.LogInformation("User registered: {Id}", user.Id);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var user = await users.ByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (
            user == null
            || hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password)
                == PasswordVerificationResult.Failed
        )
            return null;
        log.LogInformation("User logged in: {Id}", user.Id);
        return new(
            jwt.Create(user.Id, user.DisplayName, user.Email),
            user.Id,
            user.DisplayName,
            user.Email,
            user.PreferredLanguage
        );
    }
}

public class UserService(IUserRepository users) : IUserService
{
    public async Task<UserProfileDto?> GetAsync(int id)
    {
        var u = await users.ByIdAsync(id);
        return u == null ? null : new(u.Id, u.DisplayName, u.Email, u.PreferredLanguage);
    }
}

public class MeetingService(IMeetingRepository meetings) : IMeetingService
{
    private static MeetingDto Map(Meeting m) =>
        new(m.Id, m.RoomCode, m.CreatedAt, m.StartedAt, m.EndedAt, m.Participants.Count);

    public async Task<MeetingDto> CreateAsync(int uid)
    {
        string code;
        do code = RoomCodeGenerator.Generate();
        while (await meetings.ByCodeAsync(code) != null);
        var m = new Meeting
        {
            RoomCode = code,
            CreatedBy = uid,
            StartedAt = DateTime.UtcNow,
        };
        m.Participants.Add(new() { UserId = uid, SelectedLanguage = "en" });
        await meetings.AddAsync(m);
        await meetings.SaveAsync();
        return Map(m);
    }

    public async Task<MeetingDto?> GetAsync(string code)
    {
        var m = await meetings.ByCodeAsync(code.ToUpperInvariant());
        return m == null ? null : Map(m);
    }

    public async Task<MeetingDto?> JoinAsync(string code, int uid, string lang)
    {
        if (!Languages.IsSupported(lang))
            throw new InvalidOperationException("Unsupported language.");
        var m = await meetings.ByCodeAsync(code.ToUpperInvariant());
        if (m == null || m.EndedAt != null)
            return null;
        var p = m.Participants.FirstOrDefault(x => x.UserId == uid);
        if (p == null)
            m.Participants.Add(new() { UserId = uid, SelectedLanguage = lang });
        else
        {
            p.SelectedLanguage = lang;
            p.LeftAt = null;
        }
        await meetings.SaveAsync();
        return Map(m);
    }

    public async Task<bool> LeaveAsync(string code, int uid)
    {
        var m = await meetings.ByCodeAsync(code.ToUpperInvariant());
        var p = m?.Participants.FirstOrDefault(x => x.UserId == uid);
        if (p == null)
            return false;
        p.LeftAt = DateTime.UtcNow;
        if (m!.CreatedBy == uid)
            m.EndedAt = DateTime.UtcNow;
        await meetings.SaveAsync();
        return true;
    }

    public async Task<List<MeetingDto>> HistoryAsync(int uid) =>
        (await meetings.HistoryAsync(uid)).Select(Map).ToList();

    public async Task<bool> IsMemberAsync(int id, int uid)
    {
        var m = await meetings.ByIdAsync(id);
        return m != null && m.Participants.Any(x => x.UserId == uid);
    }
}

public class MockTranslationService : ITranslationService
{
    public Task<string> TranslateAsync(string text, string source, string target) =>
        Task.FromResult(
            source == target ? text : $"[{Languages.All.GetValueOrDefault(target, target)}] {text}"
        );
}

public class TranscriptService(ITranscriptRepository repo, IMeetingService meetings)
    : ITranscriptService
{
    public async Task<TranscriptDto> SaveAsync(int mid, int uid, SaveTranscriptDto dto)
    {
        if (!await meetings.IsMemberAsync(mid, uid))
            throw new UnauthorizedAccessException();
        var s = new TranscriptSegment
        {
            MeetingId = mid,
            SpeakerId = uid,
            OriginalText = dto.Text.Trim(),
            SourceLanguage = dto.SourceLanguage,
        };
        await repo.AddAsync(s);
        await repo.SaveAsync();
        return new(s.Id, uid, "", s.OriginalText, s.SourceLanguage, s.CreatedAt);
    }

    public async Task<List<TranscriptDto>?> GetAsync(int mid, int uid)
    {
        if (!await meetings.IsMemberAsync(mid, uid))
            return null;
        return (await repo.ForMeetingAsync(mid))
            .Select(x => new TranscriptDto(
                x.Id,
                x.SpeakerId,
                x.Speaker.DisplayName,
                x.OriginalText,
                x.SourceLanguage,
                x.CreatedAt
            ))
            .ToList();
    }
}
