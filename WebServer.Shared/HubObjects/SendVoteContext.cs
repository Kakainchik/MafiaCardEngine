namespace WebServer.Shared.HubObjects
{
    public record class SendVoteContext : Context
    {
        public required ulong? TargetId { get; init; }
    }
}