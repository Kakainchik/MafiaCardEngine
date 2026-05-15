using GameLogic.Cycles;
using GameLogic.ParanoiaCorp.Attributes;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public sealed class EndGameCycle : ICycle
    {
        private GameEngine engine;

        public EndGameHistory History { get; }

        public EndGameCycle(GameEngine engine, Team? winner)
        {
            this.engine = engine;
            History = new EndGameHistory
            {
                Winner = winner,
                Rounds = Array.Empty<EndGameRoundHistory>()
            };
        }

        public bool CanFinish()
        {
            return false;
        }

        public ICycle NextCycle()
        {
            return this;
        }
    }

    public record struct EndGameHistory
    {
        public Team? Winner { get; init; }
        public EndGameRoundHistory[] Rounds { get; init; }
    }

    public record struct EndGameRoundHistory
    {
        public int Turn { get; init; }
        public ulong? FiredPlayer { get; init; }
        public ulong[] CandidatesForFiring { get; init; }
        public EndGameOvernightHistory[] OvertimeActions { get; init;  }
    }

    public record struct EndGameOvernightHistory
    {
        public ulong Executor { get; init; }
        public Type ExecutorRoleType { get; init; }
        public ulong[] Targets { get; init; }
        public bool Success { get; init; }
    }
}