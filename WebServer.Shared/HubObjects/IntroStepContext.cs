namespace WebServer.Shared.HubObjects
{
    public record class IntroStepContext : Context
    {
        public required IntroStep Step { get; init; }

        public enum IntroStep : byte
        {
            START,
            MIDDLE,
            END,
            TIP
        }
    }
}