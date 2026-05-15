using DiscordBot.Model;

namespace DiscordBot
{
    public class LobbySession
    {
        /// <summary>
        /// Lobby's host ID. This user has special permissions to manage the lobby and start the game.
        /// </summary>
        public ulong HostId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }

        public int TotalRolesCount => SelectedRolesPool.Values.Sum();

        /// <summary>
        /// Player IDs currently in the lobby.
        /// </summary>
        public HashSet<ulong> Players { get; set; } = new HashSet<ulong>();
        public Dictionary<RoleVisual, int> SelectedRolesPool { get; set; } = new Dictionary<RoleVisual, int>();

        public LobbySession(ulong hostId, ulong channelId)
        {
            HostId = hostId;
            ChannelId = channelId;
            Players.Add(hostId); //The host automatically joins the lobby upon creation

            //Add the General Director role by default, as it's required for the game.
            SelectedRolesPool.Add(RoleVisual.GENERAL_DIRECTOR, 1);
        }
    }
}