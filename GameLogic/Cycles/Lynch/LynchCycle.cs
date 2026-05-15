using GameLogic.Interfaces;

namespace GameLogic.Cycles.Lynch
{
    public class LynchCycle
    {
        protected List<Player> alivePlayers;

        public ILynch Elected { get; }

        public LynchCycle(List<Player> alivePlayers, ILynch elected)
        {
            this.alivePlayers = alivePlayers;
            Elected = elected;
        }

        public void Lynch(string message)
        {
            Elected.LastMessage = message;
            Elected.Kill();
        }
    }
}