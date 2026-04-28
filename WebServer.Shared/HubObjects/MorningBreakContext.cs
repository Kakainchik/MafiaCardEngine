namespace WebServer.Shared.HubObjects
{
    public record class MorningBreakContext : Context
    {
        public required int Deaths { get; init; }
        public required int Town { get; init; }
        public required int Mafia { get; init; }
        public required int Cultus { get; init; }
        public required int Undead { get; init; }
        public required int Neutral { get; init; }
    }
}