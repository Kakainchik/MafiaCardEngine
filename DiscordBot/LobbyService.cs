using DiscordBot.Model;
using GameLogic.ParanoiaCorp;
using System.Collections.Concurrent;

namespace DiscordBot
{
    public class LobbyService
    {
        private readonly ConcurrentDictionary<ulong, LobbySession> activeLobbies = new();

        public LobbySession CreateLobby(ulong hostId, ulong channelId)
        {
            LobbySession lobby = new LobbySession(hostId, channelId);
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

            //Check for at least two fractions present

            return (true, string.Empty);
        }
    }
}