namespace WebServer.Shared.HubObjects
{
    public record class NightActionConfirmation : Context
    {
        public required int AbilityFlag { get; init; }
        public required ulong[] TargetIds { get; init; }
    }
}