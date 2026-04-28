namespace WebServer.Shared.HubObjects
{
    public record class NightPlayerDataContext : Context
    {
        public required NightPlayerInstance[] NightPlayers { get; init; }

        public sealed record NightPlayerInstance
        {
            public required long Id { get; init; }
            public required bool IsAlive { get; init; }
        }
    }
}