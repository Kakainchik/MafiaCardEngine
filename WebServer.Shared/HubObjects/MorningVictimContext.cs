using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    public record class MorningVictimContext : Context
    {
        public required ulong VictimId { get; init; }
        public required RoleSignature VictimRole { get; init; }
        public required DeathReason Reason { get; init; }
        public string? LastWill { get; init; }

        public enum DeathReason : byte
        {
            MAFIA,
            VIGILANTE,
            SERIAL_KILLER,
            TERRORIST,
            DRIVER,
            SUICIDE
        }
    }
}