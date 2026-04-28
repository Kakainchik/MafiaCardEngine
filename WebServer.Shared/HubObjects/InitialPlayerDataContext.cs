using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class InitialPlayerDataContext : Context
    {
        public required RoleSignature OwnRole { get; init; }
        public required PlayerInstance[] AllPlayers { get; init; }

        public sealed record class PlayerInstance
        {
            public required long Id { get; init; }
            public required string Nickname { get; init; }
            public required RGB NColor { get; init; }
        }
    }
}