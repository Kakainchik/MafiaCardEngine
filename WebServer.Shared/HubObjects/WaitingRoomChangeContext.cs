using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class WaitingRoomReadyChangeContext : Context
    {
        public required ulong UserId { get; init; }
        public required bool IsReady { get; init; }
    }

    public record class WaitingRoomRoleChangeContext : Context
    {
        public required RoleSignature Role { get; init; }
        public required int Amount { get; init; }
    }

    public record class WaitingRoomSeatsChangeContext : Context
    {
        public required int MaxSeats { get; init; }
    }
}