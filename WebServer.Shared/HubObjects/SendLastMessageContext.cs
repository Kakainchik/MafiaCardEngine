namespace WebServer.Shared.HubObjects
{
    public record class SendLastMessageContext : Context
    {
        public required string Message { get; init; }
    }
}