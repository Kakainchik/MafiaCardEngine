using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Windows.Threading;
using WebServer.Shared.HubObjects;
using WPFClientShell.DataSource;
using WPFClientShell.Model.API;

namespace WPFClientShell.Model.Hub
{
    public class LobbyDomain
    {
        private readonly LobbyHubDataSource dataSource;
        private readonly AuthSettings authSettings;
        private readonly JsonSerializerSettings jsonSettings;

        private Dispatcher? dispatcher;
        private string? group;
        private int roomId;

        public event Action<Context>? ContextReceived;
        public event Action<ChatContext>? ChatContextReceived;

        public LobbyDomain(LobbyHubDataSource dataSource, IOptionsMonitor<AuthSettings> authSettings)
        {
            this.dataSource = dataSource;
            this.authSettings = authSettings.CurrentValue;

            jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public async Task<WaitingRoomContext> JoinLobbyAsync(int roomId, Dispatcher dispatcher)
        {
            dataSource.EnsureHasAccessToken();

            //Make a connection to the hub
            await dataSource.ConnectAsync();

            //Get group name the user connected
            group = await dataSource.SendWithResultAsync<string?>(LobbyHubConstants.JOIN_LOBBY_METHOD, roomId);
            if(group is null)
            {
                throw new OperationCanceledException("Unable to join lobby.");
            }

            this.dispatcher = dispatcher;
            this.roomId = roomId;

            dataSource.ListenToMethod<string>(LobbyHubConstants.RECEIVE_CONTEXT_METHOD, HandleContext);
            dataSource.ListenToMethod<string>(LobbyHubConstants.RECEIVE_CHAT_CONTEXT_METHOD, HandleChatContext);

            //Get waiting room details
            ServerRequestContext request = new ServerRequestContext
            {
                RequestFor = ServerRequestType.FOR_WAITROOM_DATA
            };
            return await SendServerRequestAsync<WaitingRoomContext>(request);
        }

        public async Task LeaveLobbyAsync()
        {
            await dataSource.DisconnectAsync();
            
            dispatcher = null;
            group = null;
            roomId = 0;
            ContextReceived = null;
            ChatContextReceived = null;
        }

        public async Task SendContextAsync(Context context)
        {
            context.Presenter = SetPresenter();
            string json = JsonConvert.SerializeObject(context, jsonSettings);
            await dataSource.SendAsync(LobbyHubConstants.SEND_CONTEXT_METHOD, json);
        }

        public async Task SendChatContextAsync(ChatContext context)
        {
            context.Presenter = SetPresenter();
            string json = JsonConvert.SerializeObject(context, jsonSettings);
            await dataSource.SendAsync(LobbyHubConstants.SEND_CHAT_CONTEXT_METHOD, json);
        }

        public async Task<T> SendServerRequestAsync<T>(ServerRequestContext context)
        {
            context.Presenter = SetPresenter();
            string json = JsonConvert.SerializeObject(context, jsonSettings);
            return await dataSource.SendWithResultAsync<T>(LobbyHubConstants.SEND_SERVER_REQUEST_METHOD, json);
        }

        private void HandleContext(string json)
        {
            Context context = JsonConvert.DeserializeObject<Context>(json, jsonSettings)!;
            dispatcher?.Invoke(() => ContextReceived?.Invoke(context));
        }

        private void HandleChatContext(string json)
        {
            ChatContext context = JsonConvert.DeserializeObject<ChatContext>(json, jsonSettings)!;
            dispatcher?.Invoke(() => ChatContextReceived?.Invoke(context));
        }

        private Context.ContextPresenter SetPresenter()
        {
            return new Context.ContextPresenter
            {
                RoomId = roomId,
                Sender = authSettings.UserId
            };
        }
    }
}