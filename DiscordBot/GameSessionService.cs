using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Model;
using GameLogic;
using GameLogic.ParanoiaCorp;
using GameLogic.ParanoiaCorp.Cycles;
using System.Collections.Concurrent;
using WebServer.Shared.ParanoiaCorp.Extensions;

namespace DiscordBot
{
    public class GameSessionService
    {
        private readonly DiscordStageController stageController;
        private readonly ConcurrentDictionary<ulong, GameHolder> activeGameSessions = new();

        public GameSessionService(DiscordStageController stageController)
        {
            this.stageController = stageController;
        }

        /// <summary>
        /// Creates a new game session for the specified guild using the provided lobby. The role cards will be assigned randomly based on the selected roles pool in the lobby.
        /// </summary>
        public GameHolder CreateGameSession(SocketGuild guild, LobbySession lobby)
        {
            if(activeGameSessions.ContainsKey(guild.Id))
            {
                throw new ArgumentException("A game session is already active for this guild. " +
                    "Please abandon the existing session before creating a new one.", nameof(guild));
            }

            IList<RoleVisual> unorderedRoles = new List<RoleVisual>();
            foreach(KeyValuePair<RoleVisual, int> kvp in lobby.SelectedRolesPool)
            {
                for(int i = 0; i < kvp.Value; i++)
                {
                    unorderedRoles.Add(kvp.Key);
                }
            }

            RoleVisual[] roleCards = unorderedRoles.ToArray();
            Random.Shared.Shuffle(roleCards);
            unorderedRoles.Clear();

            Player[] logicPlayers = new Player[lobby.Players.Count];
            int index = 0;
            foreach(ulong playerId in lobby.Players)
            {
                logicPlayers[index] = new Player(playerId, roleCards[index].MapRole().MakeGameRole());
                index++;
            }

            IntroductoryDayCycle initialCycle = new IntroductoryDayCycle();
            GameEngine engine = new GameEngine(logicPlayers, initialCycle);
            GameHolder holder = new GameHolder(guild, engine);

            initialCycle.Engine = engine;

            activeGameSessions[guild.Id] = holder;
            return holder;
        }

        /// <summary>
        /// Creates a new game session for the specified guild using the provided lobby. The role cards are provided manually.
        /// </summary>
        public GameHolder CreateGameSession(SocketGuild guild, LobbySession lobby, IReadOnlyDictionary<ulong, RoleVisual> roleCards)
        {
            if(activeGameSessions.ContainsKey(guild.Id))
            {
                throw new ArgumentException("A game session is already active for this guild. " +
                    "Please abandon the existing session before creating a new one.", nameof(guild));
            }

            if(roleCards.Count != lobby.Players.Count)
            {
                throw new ArgumentException("The number of role cards must match the number of players in the lobby.",
                    nameof(roleCards));
            }

            Player[] logicPlayers = new Player[lobby.Players.Count];
            int index = 0;
            foreach(ulong playerId in lobby.Players)
            {
                logicPlayers[index] = new Player(playerId, roleCards[playerId].MapRole().MakeGameRole());
                index++;
            }

            IntroductoryDayCycle initialCycle = new IntroductoryDayCycle();
            GameEngine engine = new GameEngine(logicPlayers, initialCycle);
            GameHolder holder = new GameHolder(guild, engine);

            initialCycle.Engine = engine;

            activeGameSessions[guild.Id] = holder;
            return holder;
        }

        public GameHolder? GetGameSession(ulong hostId)
        {
            activeGameSessions.TryGetValue(hostId, out GameHolder? gameSession);
            return gameSession;
        }

        public void EndGameSession(ulong guildId)
        {
            activeGameSessions.TryRemove(guildId, out _);
        }

        public bool HasActiveSession(ulong guildId)
        {
            return activeGameSessions.ContainsKey(guildId);
        }

        public async Task StartGameSession(GameHolder holder)
        {
            await stageController.SetupEnvironmentAsync(holder);
            await stageController.SyncCycleWithDiscordAsync(holder);
        }

        public IReadOnlyDictionary<ulong, RoleVisual> GetRoleCards(ulong guildId)
        {
            IDictionary<ulong, RoleVisual> roleCards = new Dictionary<ulong, RoleVisual>();
            GameHolder? holder = GetGameSession(guildId);

            if(holder != null)
            {
                Player[] players = holder.Engine.Players;
                for(int i = 0; i < players.Length; i++)
                {
                    roleCards[players[i].Id] = players[i].Role.IntoSignature().MapRole();
                }
            }

            return roleCards.AsReadOnly();
        }

        public async Task TryFinishCycle(GameHolder holder)
        {
            if(holder.Engine.CurrentCycle.CanFinish())
            {
                holder.Engine.NextTurn();
                await stageController.SyncCycleWithDiscordAsync(holder);
            }
        }
    }
}