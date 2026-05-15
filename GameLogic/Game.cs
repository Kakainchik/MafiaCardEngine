using GameLogic.Attributes;
using GameLogic.Cycles.Day;
using GameLogic.Cycles.Lynch;
using GameLogic.Cycles.Morning;
using GameLogic.Cycles.Night;
using GameLogic.Exceptions;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;
using System.Data;

namespace GameLogic
{
    public class Game
    {
        private const string LACK_PLAYERS_ERROR = "Too few players. Minimum is 5.";
        private const string GAME_RUNNED_ERROR = "The game is already running.";
        private const string UNIQUE_ROLES_ERROR = "There are more than one certain type unique role.";

        public const int MIN_PLAYERS = 5;

        private IEnumerable<Player> previousNightAlive = new List<Player>();

        private List<Player> AlivePlayers => Players.Where(p => p.IsAlive).ToList();

        /// <summary>
        /// List of the roles in this game.
        /// </summary>
        public Player[] Players { get; }

        /// <summary>
        /// The number of the current day in the game.
        /// </summary>
        public int Day { get; private set; }
        public MorningCycle? MorningCycle { get; private set; }
        public DayCycle? DayCycle { get; private set; }
        public LynchCycle? LynchCycle { get; private set; }
        public NightCycle? NightCycle { get; private set; }

        /// <summary>
        /// Indicates if the game is started.
        /// </summary>
        public bool Started { get; private set; }

        public Game(Player[] players)
        {
            //Check on unique roles
            IEnumerable<Role> uniqueRoles = players.Select<Player, Role>(p => p.Role)
                .Where<Role>(r => r.IsUniqie());

            if(uniqueRoles.OfType<GodfatherRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }
            if(uniqueRoles.OfType<PsychicRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }
            if(uniqueRoles.OfType<CultusLeaderRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }

            Players = players;
        }

        public event EventHandler<Team?>? GameEnded;

        /// <summary>
        /// Runs the game with uploaded roles.
        /// </summary>
        public void Run()
        {
            if(Players.Length < MIN_PLAYERS)
            {
                throw new InitializingGameException(LACK_PLAYERS_ERROR);
            }

            if(Started)
            {
                throw new InitializingGameException(GAME_RUNNED_ERROR);
            }

            //Inform about starting the game
            Started = true;
            Day = 0;
        }

        public void NextTurn()
        {
            if(Day == 0)
            {
                //Run the first turn - day gathering
                DayCycle = new DayCycle(AlivePlayers, ++Day);
                return;
            }

            if(DayCycle is not null)
            {
                IVotable voted = DayCycle.GetElectionResult();
                DayCycle = null;

                if(DayCycle.IsItPenguin(voted))
                {
                    //No one elected
                    NightCycle = new NightCycle(AlivePlayers);
                    previousNightAlive = AlivePlayers;
                }
                else
                {
                    //Someone been elected to by lynched
                    LynchCycle = new LynchCycle(AlivePlayers, voted);
                }
            }
            else if(LynchCycle is not null)
            {
                LynchCycle = null;
                NightCycle = new NightCycle(AlivePlayers);
                previousNightAlive = AlivePlayers;
            }
            else if(NightCycle is not null)
            {
                NightCycle = null;
                MorningCycle = new MorningCycle(AlivePlayers, previousNightAlive);
            }
            else if(MorningCycle is not null)
            {
                if(MorningCycle.TryResolveWinner(out Team? winner))
                {
                    //A winner been found, end the game
                    MorningCycle = null;
                    GameEnded?.Invoke(this, winner);
                }
                else
                {
                    //No winner, just make new cycle
                    MorningCycle = null;
                    DayCycle = new DayCycle(AlivePlayers, ++Day);
                }
            }
        }
    }
}