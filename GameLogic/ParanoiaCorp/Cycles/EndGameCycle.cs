using GameLogic.Cycles;
using GameLogic.ParanoiaCorp.Attributes;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public sealed class EndGameCycle : ICycle
    {
        public EndGameHistory EndGameHistory { get; }

        public EndGameCycle(GameEngine engine, Team? winner)
        {
            EndGameHistory = new EndGameHistory
            {
                Winner = winner,
                Rounds = engine.History
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
        public Queue<EndGameRoundHistory> Rounds { get; init; }
    }
}