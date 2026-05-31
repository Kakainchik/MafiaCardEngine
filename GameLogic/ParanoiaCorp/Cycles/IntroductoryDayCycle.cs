using GameLogic.Cycles;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public sealed class IntroductoryDayCycle : ICycle
    {
        private readonly ISet<Player> playerReadyStatus = new HashSet<Player>();

        public GameEngine? Engine { get; set; }

        public bool CanFinish()
        {
            return playerReadyStatus.Count == Engine?.Players.Length;
        }

        public ICycle NextCycle()
        {
            if(Engine != null)
            {
                EndGameRoundHistory roundHistory = new EndGameRoundHistory()
                {
                    Turn = 0,
                };
                Engine.History.Enqueue(roundHistory);

                return new OvertimeCycle(Engine);
            }
            else
            {
                throw new InvalidOperationException("Engine is not set for the cycle.");
            }
        }

        public void SetPlayerReady(Player player)
        {
            playerReadyStatus.Add(player);
        }
    }
}