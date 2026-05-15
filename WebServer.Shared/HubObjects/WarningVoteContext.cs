namespace WebServer.Shared.HubObjects
{
    public record class WarningVoteContext : Context
    {
        public ulong? VotedId { get; init; }
    }
}