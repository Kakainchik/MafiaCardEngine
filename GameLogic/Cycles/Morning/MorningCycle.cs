using GameLogic.Attributes;
using GameLogic.Extensions;
using GameLogic.Roles;

namespace GameLogic.Cycles.Morning
{
    public class MorningCycle
    {
        private IEnumerable<Player> previousRoundAlive;

        protected List<Player> alivePlayers;

        public int TownPlayerNumber { get; }
        public int MafiaPlayerNumber { get; }
        public int CultusPlayerNumber { get; }
        public int UndeadPlayerNumber { get; }
        public int SerialKPlayerNumber { get; }
        public int WitchPlayerNumber { get; }
        public int TerroristPlayerNumber { get; }

        public int NeutralPlayerNumber => SerialKPlayerNumber
            + WitchPlayerNumber
            + TerroristPlayerNumber;

        public MorningCycle(List<Player> alivePlayers, IEnumerable<Player> previousRoundAlive)
        {
            this.alivePlayers = alivePlayers;
            this.previousRoundAlive = previousRoundAlive;

            TownPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.TOWN);
            MafiaPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.MAFIA);
            CultusPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.CULTUS);
            UndeadPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.UNDEAD);
            SerialKPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.SERIAL_KILLER);
            WitchPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.WITCH);
            TerroristPlayerNumber = alivePlayers.Count(p => p.Role.GetTeam() == Team.TERRORIST);
        }

        public ISet<DeathReason> NoteDeaths()
        {
            IEnumerable<Player> dead = previousRoundAlive.Where(p => p.DeathReason is not null);

            ISet<DeathReason> reasons = new HashSet<DeathReason>();
            foreach(Player d in dead)
            {
                DeathReason reason = new DeathReason
                {
                    Dead = d,
                    Reason = d.DeathReason!.Owner!
                };
                reasons.Add(reason);
            }

            return reasons;
        }

        public bool TryResolveWinner(out Team? result)
        {
            //Everyone is dead
            if(alivePlayers.Count == 0)
            {
                //If there was a terrorist at previous night
                if(previousRoundAlive.Any(p => p.Role is TerroristRole))
                {
                    result = Team.TERRORIST;
                    return true;
                }

                result = null;
                return true;
            }

            //Town wins when remains alone
            if(TownPlayerNumber == alivePlayers.Count)
            {
                result = Team.TOWN;
                return true;
            }

            //Check others who stays alone
            if(MafiaPlayerNumber == alivePlayers.Count)
            {
                result = Team.MAFIA;
                return true;
            }
            if(CultusPlayerNumber == alivePlayers.Count)
            {
                result = Team.CULTUS;
                return true;
            }
            if(UndeadPlayerNumber == alivePlayers.Count)
            {
                result = Team.UNDEAD;
                return true;
            }

            //Check alone neutrals
            if(SerialKPlayerNumber == alivePlayers.Count)
            {
                result = Team.SERIAL_KILLER;
                return true;
            }
            if(WitchPlayerNumber == alivePlayers.Count)
            {
                result = Team.WITCH;
                return true;
            }
            if(TerroristPlayerNumber == alivePlayers.Count)
            {
                result = Team.TERRORIST;
                return true;
            }

            //Check one-versus-one
            if(alivePlayers.Count == 2)
            {
                //Order is important because it means priority of winners
                if(SerialKPlayerNumber >= 1)
                {
                    result = Team.SERIAL_KILLER;
                }
                else if(MafiaPlayerNumber >= 1)
                {
                    result = Team.MAFIA;
                }
                else if(CultusPlayerNumber >= 1)
                {
                    result = Team.CULTUS;
                }
                else if(WitchPlayerNumber >= 1)
                {
                    result = Team.WITCH;
                }
                else
                {
                    result = Team.TOWN;
                }

                return true;
            }

            result = default;
            return false;
        }
    }
}