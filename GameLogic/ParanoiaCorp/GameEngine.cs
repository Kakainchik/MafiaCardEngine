using GameLogic.Exceptions;
using GameLogic.Extensions;
using GameLogic.ParanoiaCorp.Roles;
using GameLogic.Roles;
using GameLogic.Cycles;

namespace GameLogic.ParanoiaCorp
{
    public class GameEngine
    {
        private const string LACK_PLAYERS_ERROR = "Too few players. Minimum is 6.";
        private const string UNIQUE_ROLES_ERROR = "There are more than one certain type unique role.";

        public const int MIN_PLAYERS = 2;

        /// <summary>
        /// List of the roles in this game.
        /// </summary>
        public Player[] Players { get; }
        public GeneralDirectorRole GeneralDirector { get; }
        /// <summary>
        /// The number of the current day in the game.
        /// </summary>
        public int Day { get; internal set; }
        public ICycle CurrentCycle { get; private set; }
        public Queue<EndGameRoundHistory> History { get; }

        /// <summary>
        /// Indicates if the game is started.
        /// </summary>
        public bool Started { get; private set; }

        public List<Player> AlivePlayers => Players.Where(p => p.IsAlive && p.Role is not GeneralDirectorRole).ToList();

        public GameEngine(Player[] players, ICycle initialCycle)
        {
            //Check on required roles
            if(!players.Any(p => p.Role is GeneralDirectorRole))
            {
                throw new InitializingGameException("The game requires exactly one General Director.");
            }

            //Check on unique roles
            IEnumerable<Role> uniqueRoles = players
                .Select<Player, Role>(p => p.Role)
                .Where<Role>(r => r.IsUniqie());

            if(uniqueRoles.OfType<GeneralDirectorRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }
            if(uniqueRoles.OfType<AlumniManagerRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }
            if(uniqueRoles.OfType<StartupFounderRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }
            if(uniqueRoles.OfType<ShadowDirectorRole>().Count() > 1)
            {
                throw new InitializingGameException(UNIQUE_ROLES_ERROR);
            }

            //Check on minimum players
            if(players.Length < MIN_PLAYERS)
            {
                throw new InitializingGameException(LACK_PLAYERS_ERROR);
            }

            History = new Queue<EndGameRoundHistory>();

            GeneralDirector = (GeneralDirectorRole)players.Single(p => p.Role is GeneralDirectorRole).Role;
            Players = players;
            CurrentCycle = initialCycle;

            IEnumerable<AnticrisisManagerRole> anticrisisManagerRoles = players.Select(p => p.Role).OfType<AnticrisisManagerRole>();
            int itemsCount = (int)(players.Length / 4) + 1;
            foreach(AnticrisisManagerRole anticrisisManager in anticrisisManagerRoles)
            {
                anticrisisManager.Items = itemsCount;
            }
        }

        public void NextTurn()
        {
            CurrentCycle = CurrentCycle.NextCycle();
        }
    }

    public class EndGameRoundHistory
    {
        public int Turn { get; set; }
        public ulong? FiredPlayer { get; set; }
        public ulong[]? CandidatesForFiring { get; set; }
        public Queue<EndGameOvernightHistory>? OvertimeActions { get; set; }
    }

    public record class EndGameOvernightHistory
    {
        public required ulong Executor { get; set; }
        public required Type ExecutorRoleType { get; set; }
        public required ulong[] Targets { get; set; }
    }
}