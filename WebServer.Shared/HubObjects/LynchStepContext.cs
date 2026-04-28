namespace WebServer.Shared.HubObjects
{
    public record class LynchStepContext : Context
    {
        public required LynchStep Step { get; init; }

        public enum LynchStep : byte
        {
            QUESTION,
            LAST_MESSAGE,
            PREPARE_EXECUTE,
            EXECUTE,
            SHOW_ROLE,
            END
        }
    }
}