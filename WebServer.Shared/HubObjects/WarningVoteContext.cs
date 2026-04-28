namespace WebServer.Shared.HubObjects
{
    public record class WarningVoteContext : Context
    {
        public long? VotedId { get; init; }
    }
}