using System.Diagnostics.CodeAnalysis;

namespace WebServer.Shared.HubObjects
{
    public record class ServerChatContext : ChatContext
    {
        [SetsRequiredMembers]
        public ServerChatContext(string message) : base(string.Empty, message)
        {

        }
    }
}