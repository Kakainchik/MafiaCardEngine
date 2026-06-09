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

    public record class EndGameHistory
    {
        public Team? Winner { get; set; }
        public required Queue<EndGameRoundHistory> Rounds { get; set; }
    }
}