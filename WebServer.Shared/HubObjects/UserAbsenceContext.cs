namespace WebServer.Shared.HubObjects
{
    public record class UserAbsenceContext : Context
    {
        public required ulong UserId { get; init; }
        public required string Username { get; init; }
        public bool HasRemoved { get; init; }
        public bool HasAdded { get; init; }
    }
}