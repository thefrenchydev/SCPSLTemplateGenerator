namespace Template.Events;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;

public class EventsHandler : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
       Logger.Debug($"Player {ev.Player.Nickname} joined the game.");
    }
}