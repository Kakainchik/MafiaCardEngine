namespace WebServer.Shared.HubObjects
{
    public record class ServerRequestContext : Context
    {
        public required ServerRequestType RequestFor { get; init; }
    }

    public enum ServerRequestType
    {
        FOR_WAITROOM_DATA,
        FOR_INITIALIZE_GAME,
        FOR_PLAYERS_DATA
    }
}