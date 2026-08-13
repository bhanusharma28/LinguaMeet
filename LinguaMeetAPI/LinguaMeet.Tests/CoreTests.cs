using LinguaMeet.Api.DTO;
using LinguaMeet.Api.Helpers;
using LinguaMeet.Api.Interfaces.Repositories;
using LinguaMeet.Api.Models;
using LinguaMeet.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace LinguaMeet.Tests;

public class CoreTests
{
    [Fact]
    public void RoomCodeGenerator_CreatesSixSafeCharacters()
    {
        var code = RoomCodeGenerator.Generate();
        Assert.Equal(6, code.Length);
        Assert.Matches("^[A-Z2-9]+$", code);
    }

    [Fact]
    public async Task MeetingService_CreatesUniqueRoomAndHostParticipant()
    {
        var repo = new MemoryMeetingRepository();
        var result = await new MeetingService(repo).CreateAsync(7);
        Assert.Equal(6, result.RoomCode.Length);
        Assert.Equal(1, result.ParticipantCount);
    }

    [Fact]
    public async Task AuthenticationService_HashesPasswordAndReturnsJwt()
    {
        var repo = new MemoryUserRepository();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "Jwt:Key", "THIS_IS_A_TEST_SECRET_THAT_IS_LONG_ENOUGH_123" },
                }
            )
            .Build();
        var service = new AuthService(
            repo,
            new JwtHelper(config),
            NullLogger<AuthService>.Instance
        );
        await service.RegisterAsync(new("Student", "student@test.local", "Password@123", "en"));
        Assert.DoesNotContain("Password@123", repo.User!.PasswordHash);
        Assert.NotNull(await service.LoginAsync(new("student@test.local", "Password@123")));
    }

    private sealed class MemoryMeetingRepository : IMeetingRepository
    {
        readonly List<Meeting> items = [];

        public Task AddAsync(Meeting m)
        {
            m.Id = 1;
            items.Add(m);
            return Task.CompletedTask;
        }

        public Task<Meeting?> ByCodeAsync(string c) =>
            Task.FromResult(items.FirstOrDefault(x => x.RoomCode == c));

        public Task<Meeting?> ByIdAsync(int id) =>
            Task.FromResult(items.FirstOrDefault(x => x.Id == id));

        public Task<List<Meeting>> HistoryAsync(int id) => Task.FromResult(items);

        public Task SaveAsync() => Task.CompletedTask;
    }

    private sealed class MemoryUserRepository : IUserRepository
    {
        public User? User;

        public Task AddAsync(User u)
        {
            u.Id = 1;
            User = u;
            return Task.CompletedTask;
        }

        public Task<User?> ByEmailAsync(string e) =>
            Task.FromResult(User?.Email == e ? User : null);

        public Task<User?> ByIdAsync(int id) => Task.FromResult(User);

        public Task SaveAsync() => Task.CompletedTask;
    }
}
