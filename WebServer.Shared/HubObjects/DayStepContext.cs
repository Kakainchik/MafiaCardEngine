namespace WebServer.Shared.HubObjects
{
    public record class DayStepContext : Context
    {
        public required DayStep Step { get; init; }

        public enum DayStep : byte
        {
            START_DAY,
            START_BALLOT,
            END_BALLOT,
            FIRST_DAY_CASE
        }
    }
}