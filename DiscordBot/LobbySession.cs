using DiscordBot.Model;

namespace DiscordBot
{
    public class LobbySession
    {
        /// <summary>
        /// Lobby's host ID. This user has special permissions to manage the lobby and start the game.
        /// </summary>
        public ulong HostId { get; }
        public ulong ChannelId { get; }
        public LobbyMode Mode { get; }

        /// <summary>
        /// Player IDs currently in the lobby.
        /// </summary>
        public HashSet<ulong> Players { get; } = new HashSet<ulong>();
        public Dictionary<RoleVisual, int> SelectedRolesPool { get; } = new Dictionary<RoleVisual, int>();

        public int TotalRolesCount => SelectedRolesPool.Values.Sum();

        public LobbySession(ulong hostId, ulong channelId, LobbyMode mode)
        {
            HostId = hostId;
            ChannelId = channelId;
            Mode = mode;

            if(mode == LobbyMode.Normal)
            {
                //The host automatically joins the lobby upon creation
                Players.Add(hostId);
            }

            //Add the General Director role by default, as it's required for the game.
            SelectedRolesPool.Add(RoleVisual.GENERAL_DIRECTOR, 1);
        }
    }

    public enum LobbyMode
    {
        Normal,
        Admin
    }
}