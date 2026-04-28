using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night
{
    public record class ActionLog
    {
        public required ActionType Action { get; init; }
        public required ITarget Executor { get; init; }
        public required ITarget Target { get; init; }
        public required bool Success { get; init; }
    }
}