namespace WebServer.Shared.HubObjects
{
    public record class SendVoteContext : Context
    {
        public required long? TargetId { get; init; }
    }
}