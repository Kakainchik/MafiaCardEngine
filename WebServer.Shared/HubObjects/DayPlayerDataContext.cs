namespace WebServer.Shared.HubObjects
{
    public record class DayPlayerDataContext : Context
    {
        public required DayPlayerInstance[] DayPlayers { get; init; }

        public sealed record DayPlayerInstance
        {
            public required ulong Id { get; init; }
            public required bool IsAlive { get; init; }
        }
    }
}