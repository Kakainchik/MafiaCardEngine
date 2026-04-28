namespace WebServer.Shared.HubObjects
{
    public record class KickPlayerContext : Context
    {
        public required long UserToKickId { get; init; }
    }
}