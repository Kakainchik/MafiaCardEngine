using GameLogic.Attributes;
namespace WebServer.Shared.HubObjects
{
    public record class ScopedChatContext : ChatContext
    {
        public required ChatScope Scope { get; init; }
    }
}