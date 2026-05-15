using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    public class SendUserCommand : IHubIdCommand
    {
        private readonly Hub hub;
        private readonly JsonSerializerSettings jsonSettings;

        public SendUserCommand(Hub hub, JsonSerializerSettings jsonSettings)
        {
            this.hub = hub;
            this.jsonSettings = jsonSettings;
        }

        public void Execute(Context con, params ulong[] userIds)
        {
            ExecuteAsync(con, userIds).Wait();
        }

        public async Task ExecuteAsync(Context con, params ulong[] userIds)
        {
            string json = JsonConvert.SerializeObject(con, jsonSettings);

            string[] ids = new string[userIds.Length];
            for(int i = 0; i < userIds.Length; i++)
            {
                ids[i] = userIds[i].ToString();
            }

            await hub.Clients.Users(ids).SendAsync(LobbyHubConstants.RECEIVE_CONTEXT_METHOD, json);
        }
    }
}