using LinguaMeet.Api.Models;

namespace LinguaMeet.Api.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> ByEmailAsync(string email);
    Task<User?> ByIdAsync(int id);
    Task AddAsync(User user);
    Task SaveAsync();
}

public interface IMeetingRepository
{
    Task<Meeting?> ByCodeAsync(string code);
    Task<Meeting?> ByIdAsync(int id);
    Task<List<Meeting>> HistoryAsync(int userId);
    Task AddAsync(Meeting meeting);
    Task SaveAsync();
}

public interface ITranscriptRepository
{
    Task AddAsync(TranscriptSegment item);
    Task<List<TranscriptSegment>> ForMeetingAsync(int meetingId);
    Task SaveAsync();
}
