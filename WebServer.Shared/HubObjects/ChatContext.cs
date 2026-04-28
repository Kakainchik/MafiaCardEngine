using System.Diagnostics.CodeAnalysis;

namespace WebServer.Shared.HubObjects
{
    public record class ChatContext : Context
    {
        public required string Who { get; init; }
        public required string Message { get; init; }

        [SetsRequiredMembers]
        public ChatContext(string who, string message)
        {
            Who = who;
            Message = message;
        }

        public ChatContext()
        {

        }
    }
}