using System.Security.Claims;
using LinguaMeet.Api.DTO;
using LinguaMeet.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LinguaMeet.Api.Hubs;

[Authorize]
public class MeetingHub(
    ITranscriptService transcripts,
    ITranslationService translations,
    MeetingConnectionRegistry registry
) : Hub
{
    int UserId => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task JoinMeeting(string room, string language, bool microphoneOn, bool cameraOn)
    {
        registry.Join(Context.ConnectionId, room, language);
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
        registry.Remove(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        await Clients.OthersInGroup(room).SendAsync("ParticipantLeft", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        registry.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
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
        var recipients = registry.InRoom(room);
        var byLanguage = recipients
            .Select(x => x.Language)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                language => language,
                language => translations.TranslateAsync(text, source, language),
                StringComparer.OrdinalIgnoreCase
            );
        await Task.WhenAll(byLanguage.Values);
        await Task.WhenAll(
            recipients.Select(recipient =>
                Clients.Client(recipient.ConnectionId).SendAsync(
                "ReceiveTranscript",
                UserId,
                Context.User!.Identity!.Name,
                text,
                byLanguage[recipient.Language].Result,
                source,
                saved.CreatedAt
                )
            )
        );
    }
}
