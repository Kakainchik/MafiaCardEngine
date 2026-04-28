using WebServer.Shared.GameObjects.Night;

namespace WebServer.Shared.HubObjects
{
    public record class NightActionLogContext : Context
    {
        public required InfoType Action { get; init; }
        public required bool Success { get; init; }
    }
}