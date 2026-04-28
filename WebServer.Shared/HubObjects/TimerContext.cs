namespace WebServer.Shared.HubObjects
{
    public record class TimerContext : Context
    {
        public required short Seconds { get; init; }
        public required bool ToStart { get; init; }
    }
}