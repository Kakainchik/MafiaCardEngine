namespace WebServer.Shared.HubObjects
{
    public record class GameReadyContext : Context
    {
        public required bool Success { get; init; }
    }
}