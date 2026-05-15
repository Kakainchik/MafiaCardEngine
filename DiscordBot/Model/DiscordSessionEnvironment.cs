namespace DiscordBot.Model
{
    public class DiscordSessionEnvironment
    {
        public ulong CategoryId { get; set; }
        public ulong GeneralTextChannelId { get; set; }
        public ulong VoiceBoardChannelId { get; set; }
        public Dictionary<string, ulong> PrivateChannels { get; set; } = new();
        public Dictionary<string, ulong> FractionRoles { get; set; } = new();
        public List<ulong> ManagedRoles { get; set; } = new();
        public Stack<(ulong user, string message)> PostponedMessages { get; set; } = new();
    }
}