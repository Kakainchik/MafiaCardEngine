using GameLogic.ParanoiaCorp.Attributes;
using GameLogic.ParanoiaCorp.Extensions;
using GameLogic.Cycles;
using GameLogic.Cycles.Night;
using GameLogic.ParanoiaCorp.Roles;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public class MorningCycle : ICycle
    {
        private readonly GameEngine engine;

        private bool directorReady = false;
        private bool corporationPresent;
        private bool syndicatePresent;
        private bool startupPresent;
        private bool outsourcePresent;
        private bool singlesPresent;

        public int CorporationPlayerNumber { get; }
        public int SyndicatePlayerNumber { get; }
        public int StartupPlayerNumber { get; }
        public int OutsourcePlayerNumber { get; }
        public int SinglesPlayerNumber { get; }
        public IReadOnlyCollection<ActionLog> ActionLogs { get; }

        public MorningCycle(GameEngine engine, IReadOnlyCollection<ActionLog> actionLogs)
        {
            this.engine = engine;
            CorporationPlayerNumber = engine.AlivePlayers.Count(p => p.Role.GetTeam() == Team.CORPORATION && p.Role is not GeneralDirectorRole);
            SyndicatePlayerNumber = engine.AlivePlayers.Count(p => p.Role.GetTeam() == Team.SYNDICATE);
            StartupPlayerNumber = engine.AlivePlayers.Count(p => p.Role.GetTeam() == Team.STARTUP);
            OutsourcePlayerNumber = engine.AlivePlayers.Count(p => p.Role.GetTeam() == Team.OUTSOURCE);
            SinglesPlayerNumber = engine.AlivePlayers.Count(p => p.Role.GetTeam() == Team.SINGLES);
            corporationPresent = CorporationPlayerNumber > 0;
            syndicatePresent = SyndicatePlayerNumber > 0;
            startupPresent = StartupPlayerNumber > 0;
            outsourcePresent = OutsourcePlayerNumber > 0;
            singlesPresent = SinglesPlayerNumber > 0;
            ActionLogs = actionLogs;

            EndGameRoundHistory roundHistory = new EndGameRoundHistory()
            {
                Turn = engine.Day++,
            };
            engine.History.Enqueue(roundHistory);
        }

        public bool CanFinish()
        {
            return directorReady;
        }

        public ICycle NextCycle()
        {
            //Reset hackers that didn't act in the previous turn
            IEnumerable<Role> inactiveRoles = engine.AlivePlayers.Select(player => player.Role)
                .ExceptBy(ActionLogs.Select(log => log.Executor), role => role);
            IEnumerable<HackerRole> inactiveHackers = inactiveRoles.OfType<HackerRole>();
            foreach(HackerRole hacker in inactiveHackers)
            {
                hacker.DidActionPreviousTurn = false;
                hacker.PreviousTarget = null;
            }

            Team? winner;
            bool winnerResolved = ResolveWinner(out winner);
            if(winnerResolved)
            {
                return new EndGameCycle(engine, winner);
            }
            else
            {
                return new DirectorBoardCycle(engine);
            }
        }

        public void SetDirectorReady()
        {
            directorReady = true;
        }

        public bool ResolveWinner(out Team? winner)
        {
            //null means a draw
            winner = null;
            int[] data = new[] {
                CorporationPlayerNumber,
                SyndicatePlayerNumber,
                StartupPlayerNumber,
                OutsourcePlayerNumber,
                SinglesPlayerNumber
            };

            if(data.Sum() == 0)
            {
                winner = null;
                return true;
            }

            //If there is only one faction alive, that faction wins
            bool onlyOneFactionAlive = data.Count(n => n > 0) == 1;
            //If there are more than 2 factions alive, there is no winner yet
            bool moreThanTwoFactionsAlive = data.Count(n => n > 0) > 2;
            if(onlyOneFactionAlive)
            {
                if(corporationPresent)
                {
                    winner = Team.CORPORATION;
                    return true;
                }
                else if(syndicatePresent)
                {
                    winner = Team.SYNDICATE;
                    return true;
                }
                else if(startupPresent)
                {
                    winner = Team.STARTUP;
                    return true;
                }
                else if(outsourcePresent)
                {
                    winner = Team.OUTSOURCE;
                    return true;
                }
                else
                {
                    winner = Team.SINGLES;
                    return true;
                }
            }
            else if(moreThanTwoFactionsAlive)
            {
                return false;
            }
            else
            {
                //If there are exactly 2 factions alive
                int sum = data.Sum();
                if(sum == 2)
                {
                    if(singlesPresent)
                    {
                        winner = Team.SINGLES;
                        return true;
                    }

                    //Corporation always fails in 1 vs 1
                    if(corporationPresent && syndicatePresent)
                    {
                        winner = Team.SYNDICATE;
                        return true;
                    }
                    else if(corporationPresent && startupPresent)
                    {
                        winner = Team.STARTUP;
                        return true;
                    }
                    else
                    {
                        winner = Team.OUTSOURCE;
                        return true;
                    }
                }
                else if(!corporationPresent)
                {
                    //If there are more than 2 players alive, check if one faction has the last player alive
                    //A faction with one player left fails
                    if(syndicatePresent && outsourcePresent)
                    {
                        winner = SyndicatePlayerNumber == 1 ? Team.OUTSOURCE : Team.SYNDICATE;
                    }
                    else if(syndicatePresent && startupPresent)
                    {
                        winner = SyndicatePlayerNumber == 1 ? Team.STARTUP : Team.SYNDICATE;
                    }
                    else if(startupPresent && outsourcePresent)
                    {
                        winner = StartupPlayerNumber == 1 ? Team.OUTSOURCE : Team.STARTUP;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}