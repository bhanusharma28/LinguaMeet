using LinguaMeet.Api.DTO;

namespace LinguaMeet.Api.Interfaces.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequestDto dto);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
}

public interface IUserService
{
    Task<UserProfileDto?> GetAsync(int id);
}

public interface IMeetingService
{
    Task<MeetingDto> CreateAsync(int userId);
    Task<MeetingDto?> GetAsync(string code);
    Task<MeetingDto?> JoinAsync(string code, int userId, string language);
    Task<bool> LeaveAsync(string code, int userId);
    Task<List<MeetingDto>> HistoryAsync(int userId);
    Task<bool> IsMemberAsync(int meetingId, int userId);
}

public interface ITranscriptService
{
    Task<TranscriptDto> SaveAsync(int meetingId, int speakerId, SaveTranscriptDto dto);
    Task<List<TranscriptDto>?> GetAsync(int meetingId, int userId);
}

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);
}
