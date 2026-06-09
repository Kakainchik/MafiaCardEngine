using DiscordBot.Extensions;
using DiscordBot.Model;
using GameLogic.ParanoiaCorp;
using System.Collections.Concurrent;

namespace DiscordBot
{
    public class LobbyService
    {
        private readonly ConcurrentDictionary<ulong, LobbySession> activeLobbies = new();

        public LobbySession CreateLobby(ulong hostId, ulong channelId, LobbyMode mode)
        {
            LobbySession lobby;
            switch(mode)
            {
                case LobbyMode.Normal:
                    lobby = new LobbySession(hostId, channelId, mode);
                    break;
                case LobbyMode.Admin:
                    lobby = new AdminLobbySession(hostId, channelId, mode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), $"Unsupported lobby mode: {mode}");
            }
            return lobby;
        }

        public void RegisterLobbyMessage(ulong messageId, LobbySession lobby)
        {
            activeLobbies[messageId] = lobby;
        }

        public LobbySession GetLobby(ulong messageId)
        {
            activeLobbies.TryGetValue(messageId, out LobbySession? lobby);
            return lobby!;
        }

        public void RemoveLobby(ulong messageId)
        {
            activeLobbies.TryRemove(messageId, out _);
        }

        public (bool IsValid, string ErrorMessage) ValidateLobby(LobbySession lobby)
        {
            if(lobby.Players.Count < GameEngine.MIN_PLAYERS)
            {
                return (false, $"For the game to start, at least {GameEngine.MIN_PLAYERS} employees are required.");
            }

            if(lobby.TotalRolesCount != lobby.Players.Count)
            {
                return (false, $"The number of roles ({lobby.TotalRolesCount}) does not match the number of players ({lobby.Players.Count}).");
            }

            if(!lobby.SelectedRolesPool.ContainsKey(RoleVisual.GENERAL_DIRECTOR) || lobby.SelectedRolesPool[RoleVisual.GENERAL_DIRECTOR] != 1)
            {
                return (false, "There must be exactly one General Director in the game.");
            }

            if(lobby.SelectedRolesPool.Keys.Select(role => role.GetTeam()).Distinct().Count() < 2)
            {
                return (false, "There must be at least two different fractions in the game.");
            }

            return (true, string.Empty);
        }
    }
}