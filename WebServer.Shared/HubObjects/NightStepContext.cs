namespace WebServer.Shared.HubObjects
{
    public record class NightStepContext : Context
    {
        public required NightStep Step { get; init; }

        public enum NightStep : byte
        {
            START_REMINDER,
            ALLOW_SELECTION,
            DISALLOW_SELECTION,
            END
        }
    }
}