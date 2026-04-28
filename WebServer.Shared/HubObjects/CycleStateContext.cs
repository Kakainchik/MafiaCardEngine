using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class CycleStateContext : Context
    {
        public required StageType Cycle { get; init; }
        public required bool IsAlive { get; init; }
        public required RoleSignature Role { get; init; }
    }
}