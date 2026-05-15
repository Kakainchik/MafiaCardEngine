using GameLogic.Cycles;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public class DirectorBoardCycle : ICycle
    {
        private readonly GameEngine engine;

        private Player? chosenCandidate;

        public bool VetoUsed { get; private set; }
        public IEnumerable<Player> CandidatesForFiring { get; }

        public bool CanSkipTurn => !CandidatesForFiring.Any() || VetoUsed;

        public DirectorBoardCycle(GameEngine engine)
        {
            this.engine = engine;
            CandidatesForFiring = engine.AlivePlayers.Where(player => !player.Role.IsAlive);
        }

        public bool CanFinish()
        {
            if(CanSkipTurn)
            {
                return true;
            }
            else return chosenCandidate is not null;
        }

        public ICycle NextCycle()
        {
            for(int i = 0; i < engine.AlivePlayers.Count; i++)
            {
                if(engine.AlivePlayers[i] != chosenCandidate)
                {
                    engine.AlivePlayers[i].Role.IsAlive = true;
                }
            }

            if(chosenCandidate is null)
            {
                return new OvertimeCycle(engine);
            }
            else
            {
                return new EveningCycle(engine, chosenCandidate);
            }
        }

        public bool UseVeto()
        {
            if(engine.GeneralDirector.VetoCount > 0 && !VetoUsed)
            {
                engine.GeneralDirector.VetoCount--;
                VetoUsed = true;
                return true;
            }
            else return false;
        }

        public void ChooseCandidate(Player? player)
        {
            chosenCandidate = player;
        }
    }
}