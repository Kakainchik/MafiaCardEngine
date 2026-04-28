using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Security.Claims;
using WebServer.Model.User;
using WebServer.Services;
using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        private readonly IHallService hallService;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IContextManager contextManager;
        private readonly RequestManager requestManager;
        private readonly IHubCommand sendOthersCommand;
        private readonly IHubCommand sendAllCommand;
        private readonly IHubIdCommand sendUserCommand;

        public LobbyHub(IHallService hallService,
            IContextManager contextManager,
            RequestManager requestManager)
        {
            this.hallService = hallService;
            this.contextManager = contextManager;
            this.requestManager = requestManager;

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

        public async Task<string?> Join(int roomId)
        {
            UserDTO user = UnboxClaims();
            bool isHost;
            bool joined = hallService.JoinRoom(roomId, user, out isHost);
            string? groupName = null;

            if(joined)
            {
                groupName = LobbyHubConstants.MapGroupName(roomId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                if(!isHost)
                {
                    //Notify lobby players that one joined
                    UserAbsenceContext context = new UserAbsenceContext
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        HasAdded =  true,
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

            //The id of lobby where player been removed
            int roomId = hallService.KickPlayer(user);
            string groupName = LobbyHubConstants.MapGroupName(roomId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            //Notify lobby players that one left
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
            if(context is null)
            {
                throw new JsonSerializationException("Unable deserialize Context message.");
            }
            await contextManager.ProcessContext(context);
        }

        public async Task SendChatContext(string json)
        {
            ChatContext? context = JsonConvert.DeserializeObject<ChatContext>(json, jsonSettings);
            if(context is null)
            {
                throw new JsonSerializationException("Unable deserialize Context message.");
            }

            string groupName = LobbyHubConstants.MapGroupName(context.Presenter.RoomId);
            await Clients.OthersInGroup(groupName).SendAsync(LobbyHubConstants.RECEIVE_CHAT_CONTEXT_METHOD, json);
        }

        public async Task<Context> SendServerRequest(string json)
        {
            ServerRequestContext? context = JsonConvert.DeserializeObject<ServerRequestContext>(json, jsonSettings);
            if(context is null)
            {
                throw new JsonSerializationException("Unable deserialize Context message.");
            }
            return await requestManager.ProcessContext(context);
        }

        private UserDTO UnboxClaims()
        {
            long userId = long.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string username = Context.User!.FindFirstValue(ClaimTypes.Name)!;
            return new UserDTO(userId, username);
        }
    }
}