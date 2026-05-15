namespace WebServer.Shared.HubObjects
{
    public record class KickPlayerContext : Context
    {
        public required ulong UserToKickId { get; init; }
    }
}