namespace WebServer.Shared.HubObjects
{
    public record class NightItemsContext : Context
    {
        public required int Amount { get; init; }
    }
}