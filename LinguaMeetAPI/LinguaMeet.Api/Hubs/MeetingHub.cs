using System.Security.Claims;
using LinguaMeet.Api.DTO;
using LinguaMeet.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LinguaMeet.Api.Hubs;

[Authorize]
public class MeetingHub(ITranscriptService transcripts, ITranslationService translations) : Hub
{
    int UserId => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task JoinMeeting(string room, string language, bool microphoneOn, bool cameraOn)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        await Clients
            .OthersInGroup(room)
            .SendAsync(
                "ParticipantJoined",
                Context.ConnectionId,
                UserId,
                Context.User!.Identity!.Name,
                language,
                microphoneOn,
                cameraOn
            );
    }

    public Task UpdateMediaState(string room, bool microphoneOn, bool cameraOn) =>
        Clients
            .OthersInGroup(room)
            .SendAsync("MediaStateChanged", Context.ConnectionId, microphoneOn, cameraOn);

    public async Task LeaveMeeting(string room)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        await Clients.OthersInGroup(room).SendAsync("ParticipantLeft", Context.ConnectionId);
    }

    public Task SendOffer(
        string room,
        string target,
        string offer,
        bool microphoneOn,
        bool cameraOn
    ) =>
        Clients
            .Client(target)
            .SendAsync(
                "ReceiveOffer",
                Context.ConnectionId,
                UserId,
                Context.User!.Identity!.Name,
                offer,
                microphoneOn,
                cameraOn
            );

    public Task SendAnswer(string room, string target, string answer) =>
        Clients.Client(target).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);

    public Task SendIceCandidate(string room, string target, string candidate) =>
        Clients.Client(target).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);

    public async Task SendTranscript(
        string room,
        int meetingId,
        string text,
        string source,
        string target
    )
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        var saved = await transcripts.SaveAsync(meetingId, UserId, new(text, source));
        var translated = await translations.TranslateAsync(text, source, target);
        await Clients
            .Group(room)
            .SendAsync(
                "ReceiveTranscript",
                UserId,
                Context.User!.Identity!.Name,
                text,
                translated,
                source,
                saved.CreatedAt
            );
    }
}
