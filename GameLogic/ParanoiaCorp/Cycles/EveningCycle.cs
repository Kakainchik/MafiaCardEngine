using GameLogic.Cycles;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public class EveningCycle : ICycle
    {
        private readonly GameEngine engine;

        public Player Elected { get; }

        public EveningCycle(GameEngine engine, Player fired)
        {
            this.engine = engine;
            Elected = fired;

            EndGameRoundHistory roundHistory = engine.History.Peek();
            roundHistory.FiredPlayer = Elected.Id;
        }

        public void Fire()
        {
            Elected.Kill();
        }

        public bool CanFinish()
        {
            return !Elected.IsAlive;
        }

        public ICycle NextCycle()
        {
            return new OvertimeCycle(engine);
        }
    }
}