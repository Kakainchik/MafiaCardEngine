using GameLogic;
using GameLogic.ParanoiaCorp;
using Microsoft.AspNetCore.SignalR;
using WebServer.Hubs;
using WebServer.Model.Game;
using WebServer.Model.ParanoiaCorp;
using WebServer.Model.Room;
using WebServer.Model.User;
using WebServer.Shared.Extensions;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;
using WebServer.Shared.ParanoiaCorp.Extensions;
using ParanoiaRoleSignature = WebServer.Shared.ParanoiaCorp.GameObjects.RoleSignature;

namespace WebServer.Services
{
    public interface IParanoiaLobbyService
    {
        ParanoiaRoomDTO GetLobby(int roomId);
        void SetRole(int roomId, ParanoiaRoleSignature role, int count);
        void ClearRoles(int roomId);
        (bool IsValid, List<string> Errors) ValidateLobby(int roomId);
        Task<ParanoiaGameSession> StartGameAsync(int roomId, ulong hostId);
    }

    public class ParanoiaLobbyService : IParanoiaLobbyService
    {
        private readonly IWaitRoomRepository roomRepository;
        private readonly IParanoiaGameRepository gameRepository;
        private readonly IHubContext<LobbyHub> hubContext;

        public ParanoiaLobbyService(
            IWaitRoomRepository roomRepository,
            IParanoiaGameRepository gameRepository,
            IHubContext<LobbyHub> hubContext)
        {
            this.roomRepository = roomRepository;
            this.gameRepository = gameRepository;
            this.hubContext = hubContext;
        }

        public ParanoiaRoomDTO GetLobby(int roomId)
        {
            WaitRoomDomain? room = roomRepository.GetRoom(roomId);
            if (room == null)
            {
                throw new KeyNotFoundException($"Room {roomId} not found.");
            }

            var validation = ValidateLobby(roomId);
            var rolesPool = roomRepository.GetParanoiaRoles(roomId)
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);

            var players = room.Players.Values.Select(p => new ParanoiaRoomPlayerDTO
            {
                Id = p.Id,
                Username = p.Username,
                IsReady = p.IsReady,
                IsHost = p.Id == room.Host.Id
            }).ToList();

            return new ParanoiaRoomDTO
            {
                Id = room.Id,
                Title = room.Title,
                Description = room.Description,
                Host = new UserDTO(room.Host.Id, room.Host.Username),
                Players = players,
                MaxSeats = room.MaxSeats,
                RolesPool = rolesPool,
                TotalRolesCount = rolesPool.Values.Sum(),
                IsValid = validation.IsValid,
                ValidationErrors = validation.Errors,
                IsGameStarted = gameRepository.HasGame(roomId)
            };
        }

        public void SetRole(int roomId, ParanoiaRoleSignature role, int count)
        {
            roomRepository.SetParanoiaRole(roomId, role, count);
        }

        public void ClearRoles(int roomId)
        {
            roomRepository.ClearParanoiaRoles(roomId);
        }

        public (bool IsValid, List<string> Errors) ValidateLobby(int roomId)
        {
            var errors = new List<string>();
            WaitRoomDomain? room = roomRepository.GetRoom(roomId);
            if (room == null)
            {
                errors.Add("Room does not exist.");
                return (false, errors);
            }

            int playersCount = room.Players.Count;
            if (playersCount < GameEngine.MIN_PLAYERS)
            {
                errors.Add($"At least {GameEngine.MIN_PLAYERS} employees are required to convene the board.");
            }

            var rolesPool = roomRepository.GetParanoiaRoles(roomId);
            int totalRoles = rolesPool.Values.Sum();

            if (totalRoles != playersCount)
            {
                errors.Add($"Headcount mismatch: {totalRoles} configured roles vs {playersCount} seated employees.");
            }

            if (!rolesPool.TryGetValue(ParanoiaRoleSignature.GENERAL_DIRECTOR, out int gdCount) || gdCount != 1)
            {
                errors.Add("Statutory requirement: Exactly one General Director must be seated.");
            }

            int distinctFactions = rolesPool.Keys
                .Select(r => r.GetTeam())
                .Distinct()
                .Count();

            if (distinctFactions < 2)
            {
                errors.Add("Conflict of interest requirement: At least two distinct corporate factions must be present.");
            }

            return (errors.Count == 0, errors);
        }

        public async Task<ParanoiaGameSession> StartGameAsync(int roomId, ulong hostId)
        {
            WaitRoomDomain? room = roomRepository.GetRoom(roomId);
            if (room == null)
            {
                throw new KeyNotFoundException($"Room {roomId} not found.");
            }

            if (room.Host.Id != hostId)
            {
                throw new UnauthorizedAccessException("Only the convening host may commence the proceedings.");
            }

            var validation = ValidateLobby(roomId);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Cannot convene board: {string.Join("; ", validation.Errors)}");
            }

            var rolesPool = roomRepository.GetParanoiaRoles(roomId);
            var unorderedRoles = new List<ParanoiaRoleSignature>();
            foreach (var kvp in rolesPool)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    unorderedRoles.Add(kvp.Key);
                }
            }

            ParanoiaRoleSignature[] shuffledRoles = unorderedRoles.ToArray();
            Random.Shared.Shuffle(shuffledRoles);

            ulong[] playerIds = room.Players.Keys.ToArray();
            PlayerDomain[] domains = new PlayerDomain[playerIds.Length];
            RGB[] colors = ColorBank.GetRandomColors(playerIds.Length);

            for (int i = 0; i < playerIds.Length; i++)
            {
                var role = shuffledRoles[i].MakeGameRole();
                var logicPlayer = new Player(playerIds[i], role);
                string username = room.Players[playerIds[i]].Username;
                domains[i] = new PlayerDomain(username, colors[i], logicPlayer);
            }

            var session = new ParanoiaGameSession(roomId, domains, hubContext);
            gameRepository.AddGame(roomId, session);

            // Notify all players in lobby that game has started
            string groupName = LobbyHubConstants.MapGroupName(roomId);
            await hubContext.Clients.Group(groupName).SendAsync("ParanoiaGameStarted", roomId);

            return session;
        }
    }
}
