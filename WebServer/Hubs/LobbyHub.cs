using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Security.Claims;
using WebServer.Model.ParanoiaCorp;
using WebServer.Model.Room;
using WebServer.Model.User;
using WebServer.Services;
using WebServer.Shared.HubObjects;
using ParanoiaRoleSignature = WebServer.Shared.ParanoiaCorp.GameObjects.RoleSignature;

namespace WebServer.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        private readonly IHallService hallService;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IContextManager contextManager;
        private readonly RequestManager requestManager;
        private readonly IWaitRoomRepository roomRepository;
        private readonly IParanoiaGameRepository paranoiaGameRepository;
        private readonly IServiceProvider serviceProvider;

        private readonly IHubCommand sendOthersCommand;
        private readonly IHubCommand sendAllCommand;
        private readonly IHubIdCommand sendUserCommand;

        public LobbyHub(
            IHallService hallService,
            IContextManager contextManager,
            RequestManager requestManager,
            IWaitRoomRepository roomRepository,
            IParanoiaGameRepository paranoiaGameRepository,
            IServiceProvider serviceProvider)
        {
            this.hallService = hallService;
            this.contextManager = contextManager;
            this.requestManager = requestManager;
            this.roomRepository = roomRepository;
            this.paranoiaGameRepository = paranoiaGameRepository;
            this.serviceProvider = serviceProvider;

            jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            sendOthersCommand = new SendOthersCommand(this, jsonSettings);
            sendAllCommand = new SendAllCommand(this, jsonSettings);
            sendUserCommand = new SendUserCommand(this, jsonSettings);

            this.contextManager.SendOtherCommand = sendOthersCommand;
            this.contextManager.SendAllCommand = sendAllCommand;
            this.contextManager.SendUserCommand = sendUserCommand;
            this.requestManager.SendOtherCommand = sendOthersCommand;
            this.requestManager.SendAllCommand = sendAllCommand;
            this.requestManager.SendUserCommand = sendUserCommand;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Leave();
            await base.OnDisconnectedAsync(exception);
        }

        #region Classic Mafia Hub Methods (Preserved)

        public async Task<string?> Join(int roomId)
        {
            UserDTO user = UnboxClaims();
            bool isHost;
            bool joined = hallService.JoinRoom(roomId, user, out isHost);
            string? groupName = null;

            if (joined)
            {
                groupName = LobbyHubConstants.MapGroupName(roomId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                if (!isHost)
                {
                    UserAbsenceContext context = new UserAbsenceContext
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        HasAdded = true,
                        Presenter = LobbyHubConstants.ServerPresenter(roomId)
                    };
                    string json = JsonConvert.SerializeObject(context, jsonSettings);
                    await Clients.OthersInGroup(groupName).SendAsync(LobbyHubConstants.RECEIVE_CONTEXT_METHOD, json);
                }
            }

            return groupName;
        }

        public async Task Leave()
        {
            UserDTO user = UnboxClaims();

            int roomId = hallService.KickPlayer(user);
            string groupName = LobbyHubConstants.MapGroupName(roomId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            UserAbsenceContext context = new UserAbsenceContext
            {
                UserId = user.Id,
                Username = user.Username,
                HasRemoved = true,
                Presenter = LobbyHubConstants.ServerPresenter(roomId)
            };
            string json = JsonConvert.SerializeObject(context, jsonSettings);
            await Clients.OthersInGroup(groupName).SendAsync(LobbyHubConstants.RECEIVE_CONTEXT_METHOD, json);
        }

        public async Task SendContext(string json)
        {
            Context? context = JsonConvert.DeserializeObject<Context>(json, jsonSettings);
            if (context is null)
            {
                throw new JsonSerializationException("Unable deserialize Context message.");
            }
            await contextManager.ProcessContext(context);
        }

        public async Task SendChatContext(string json)
        {
            ChatContext? context = JsonConvert.DeserializeObject<ChatContext>(json, jsonSettings);
            if (context is null)
            {
                throw new JsonSerializationException("Unable deserialize Context message.");
            }

            string groupName = LobbyHubConstants.MapGroupName(context.Presenter.RoomId);
            await Clients.OthersInGroup(groupName).SendAsync(LobbyHubConstants.RECEIVE_CHAT_CONTEXT_METHOD, json);
        }

        public async Task<Context> SendServerRequest(string json)
        {
            ServerRequestContext? context = JsonConvert.DeserializeObject<ServerRequestContext>(json, jsonSettings);
            if (context is null)
            {
                throw new JsonSerializationException("Unable deserialize Context message.");
            }
            return await requestManager.ProcessContext(context);
        }

        #endregion

        #region Paranoia Corp Real-Time Methods

        public async Task<string> JoinParanoiaRoom(int roomId)
        {
            UserDTO user = UnboxClaims();
            string groupName = LobbyHubConstants.MapGroupName(roomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Join lobby in room repository if not already in
            hallService.JoinRoom(roomId, user, out _);

            // Notify all players in lobby that room was updated
            await Clients.Group(groupName).SendAsync("ParanoiaRoomUpdated", roomId);

            // If game is active, notify state changed
            if (paranoiaGameRepository.HasGame(roomId))
            {
                await Clients.Caller.SendAsync("ParanoiaStateChanged", roomId);
            }

            return groupName;
        }

        public async Task LeaveParanoiaRoom(int roomId)
        {
            UserDTO user = UnboxClaims();
            string groupName = LobbyHubConstants.MapGroupName(roomId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            hallService.KickPlayer(user);
            await Clients.Group(groupName).SendAsync("ParanoiaRoomUpdated", roomId);
        }

        public async Task ToggleParanoiaReady(int roomId)
        {
            UserDTO user = UnboxClaims();
            WaitRoomDomain? room = roomRepository.GetRoom(roomId);
            if (room != null && room.Players.TryGetValue(user.Id, out var player))
            {
                roomRepository.SetReadiness(roomId, user.Id, !player.IsReady);
                string groupName = LobbyHubConstants.MapGroupName(roomId);
                await Clients.Group(groupName).SendAsync("ParanoiaRoomUpdated", roomId);
            }
        }

        public async Task UpdateParanoiaRole(int roomId, string role, int count)
        {
            if (Enum.TryParse<ParanoiaRoleSignature>(role, true, out var roleSig))
            {
                roomRepository.SetParanoiaRole(roomId, roleSig, count);
                string groupName = LobbyHubConstants.MapGroupName(roomId);
                await Clients.Group(groupName).SendAsync("ParanoiaRoomUpdated", roomId);
            }
        }

        public async Task ClearParanoiaRoles(int roomId)
        {
            roomRepository.ClearParanoiaRoles(roomId);
            string groupName = LobbyHubConstants.MapGroupName(roomId);
            await Clients.Group(groupName).SendAsync("ParanoiaRoomUpdated", roomId);
        }

        public async Task StartParanoiaGame(int roomId)
        {
            UserDTO user = UnboxClaims();
            var paranoiaLobbyService = serviceProvider.GetRequiredService<IParanoiaLobbyService>();
            await paranoiaLobbyService.StartGameAsync(roomId, user.Id);
        }

        public async Task SubmitIntroReady(int roomId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleIntroReadyAsync(user.Id);
            }
        }

        public async Task SubmitOvertimeAction(int roomId, ulong[] targets, bool isSkip)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleOvertimeActionAsync(user.Id, targets, isSkip);
            }
        }

        public async Task SubmitOvertimeReady(int roomId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleOvertimeReadyAsync(user.Id);
            }
        }

        public async Task SubmitDirectorStartBoard(int roomId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleDirectorStartBoardAsync(user.Id);
            }
        }

        public async Task SubmitDirectorSelectCandidate(int roomId, ulong candidateId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleDirectorSelectCandidateAsync(user.Id, candidateId);
            }
        }

        public async Task SubmitDirectorUseVeto(int roomId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleDirectorUseVetoAsync(user.Id);
            }
        }

        public async Task SubmitDirectorFire(int roomId, ulong candidateId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleDirectorFireAsync(user.Id, candidateId);
            }
        }

        public async Task SubmitDirectorSkip(int roomId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleDirectorSkipAsync(user.Id);
            }
        }

        public async Task SubmitEveningFinish(int roomId)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.HandleEveningFinishAsync(user.Id);
            }
        }

        public async Task SendParanoiaChat(int roomId, string scope, string message)
        {
            UserDTO user = UnboxClaims();
            var session = paranoiaGameRepository.GetGame(roomId);
            if (session != null)
            {
                await session.SendChatMessageAsync(user.Id, scope, message);
            }
        }

        #endregion

        private UserDTO UnboxClaims()
        {
            ulong userId = ulong.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string username = Context.User!.FindFirstValue(ClaimTypes.Name)!;
            return new UserDTO(userId, username);
        }
    }
}
