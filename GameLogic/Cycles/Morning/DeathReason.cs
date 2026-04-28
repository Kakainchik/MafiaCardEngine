using GameLogic.Interfaces;

namespace GameLogic.Cycles.Morning
{
    public record class DeathReason
    {
        public required Player Dead { get; init; }
        public required IRoleOwner Reason { get; init; }
    }
}