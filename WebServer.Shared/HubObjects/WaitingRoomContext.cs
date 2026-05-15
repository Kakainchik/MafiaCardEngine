using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class WaitingRoomContext : Context
    {
        public required IReadOnlyDictionary<RoleSignature, int> Roles { get; init; }
        public required WaitingRoomUser[] Players { get; init; }
        public required int MaxSeats { get; init; }

        public sealed record class WaitingRoomUser
        {
            public required ulong Id { get; init; }
            public required string Username { get; init; }
            public required bool IsReady { get; init; }
        }
    }
}