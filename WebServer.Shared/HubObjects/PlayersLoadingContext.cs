namespace WebServer.Shared.HubObjects
{
    public record class PlayersLoadingContext : Context
    {
        public required string[] Nicknames { get; init; }
    }
}