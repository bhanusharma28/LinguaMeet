using LinguaMeet.Api.Data;
using LinguaMeet.Api.Interfaces.Repositories;
using LinguaMeet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LinguaMeet.Api.Repository;

public class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public Task<User?> ByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> ByIdAsync(int id) => db.Users.FindAsync(id).AsTask();

    public Task AddAsync(User user) => db.Users.AddAsync(user).AsTask();

    public Task SaveAsync() => db.SaveChangesAsync();
}

public class MeetingRepository(ApplicationDbContext db) : IMeetingRepository
{
    public Task<Meeting?> ByCodeAsync(string code) =>
        db.Meetings.Include(x => x.Participants).FirstOrDefaultAsync(x => x.RoomCode == code);

    public Task<Meeting?> ByIdAsync(int id) =>
        db.Meetings.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<Meeting>> HistoryAsync(int userId) =>
        db
            .Meetings.Include(x => x.Participants)
            .Where(x => x.CreatedBy == userId || x.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task AddAsync(Meeting meeting) => db.Meetings.AddAsync(meeting).AsTask();

    public Task SaveAsync() => db.SaveChangesAsync();
}

public class TranscriptRepository(ApplicationDbContext db) : ITranscriptRepository
{
    public Task AddAsync(TranscriptSegment item) => db.TranscriptSegments.AddAsync(item).AsTask();

    public Task<List<TranscriptSegment>> ForMeetingAsync(int id) =>
        db
            .TranscriptSegments.Include(x => x.Speaker)
            .Where(x => x.MeetingId == id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public Task SaveAsync() => db.SaveChangesAsync();
}
