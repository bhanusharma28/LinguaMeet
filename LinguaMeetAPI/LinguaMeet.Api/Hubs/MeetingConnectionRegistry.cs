using System.Collections.Concurrent;
using LinguaMeet.Api.Helpers;

namespace LinguaMeet.Api.Hubs;

public sealed record MeetingConnection(string ConnectionId, string Room, string Language);

public sealed class MeetingConnectionRegistry
{
    private readonly ConcurrentDictionary<string, MeetingConnection> connections = new();

    public void Join(string connectionId, string room, string language)
    {
        var normalizedRoom = room.Trim().ToUpperInvariant();
        var normalizedLanguage = language.Trim().ToLowerInvariant();
        if (!Languages.IsSupported(normalizedLanguage))
            normalizedLanguage = "en";
        connections[connectionId] = new(connectionId, normalizedRoom, normalizedLanguage);
    }

    public void Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    public IReadOnlyList<MeetingConnection> InRoom(string room)
    {
        var normalizedRoom = room.Trim().ToUpperInvariant();
        return connections.Values.Where(x => x.Room == normalizedRoom).ToList();
    }
}
