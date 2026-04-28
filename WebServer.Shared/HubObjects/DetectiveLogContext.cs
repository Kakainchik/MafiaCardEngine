using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class DetectiveLogContext : NightActionLogContext
    {
        public required RoleSignature Target { get; init; }
    }
}