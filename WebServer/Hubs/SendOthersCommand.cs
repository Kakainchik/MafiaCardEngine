using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    public class SendOthersCommand : IHubCommand
    {
        private readonly Hub hub;
        private readonly JsonSerializerSettings jsonSettings;

        public SendOthersCommand(Hub hub, JsonSerializerSettings jsonSettings)
        {
            this.hub = hub;
            this.jsonSettings = jsonSettings;
        }

        public void Execute(Context con)
        {
            ExecuteAsync(con).Wait();
        }

        public async Task ExecuteAsync(Context con)
        {
            string groupName = LobbyHubConstants.MapGroupName(con.Presenter.RoomId);
            string json = JsonConvert.SerializeObject(con, jsonSettings);

            await hub.Clients.OthersInGroup(groupName).SendAsync(LobbyHubConstants.RECEIVE_CONTEXT_METHOD, json);
        }
    }
}