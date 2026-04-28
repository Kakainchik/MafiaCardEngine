using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class LynchPlayerContext : Context
    {
        public required long PlayerId { get; init; }
        public required string Nickname { get; init; }
        public required RGB NColor { get; init; }
        public required RoleSignature Role {  get; init; }
    }
}