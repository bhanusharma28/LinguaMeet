using LinguaMeet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LinguaMeet.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();
    public DbSet<TranscriptSegment> TranscriptSegments => Set<TranscriptSegment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Meeting>().HasIndex(x => x.RoomCode).IsUnique();
        b.Entity<Meeting>()
            .HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<MeetingParticipant>().HasIndex(x => new { x.MeetingId, x.UserId }).IsUnique();
        b.Entity<TranscriptSegment>()
            .HasOne(x => x.Speaker)
            .WithMany()
            .HasForeignKey(x => x.SpeakerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
