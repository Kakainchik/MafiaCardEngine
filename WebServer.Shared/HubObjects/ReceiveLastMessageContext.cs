namespace WebServer.Shared.HubObjects
{
    public record class ReceiveLastMessageContext : Context
    {
        public required string Message { get; init; }
    }
}