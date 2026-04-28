namespace WebServer.Shared.HubObjects
{
    public record class ReceiveVoteContext : Context
    {
        public required long Voter { get; init; }
        public required VoteTarget? PreviousTarget { get; init; }
        public required VoteTarget? CurrentTarget { get; init; }

        public sealed record class VoteTarget
        {
            public required long TargetId { get; init; }
            public required int VotesNumber { get; init; }
        }
    }
}