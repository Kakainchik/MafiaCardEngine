using DiscordBot.Model;

namespace DiscordBot
{
    public class AdminLobbySession : LobbySession
    {
        public ulong PendingSelectionPlayerId { get; set; }
        public IDictionary<ulong, RoleVisual> AttachedRoles { get; }
        public IList<RoleVisual> PreservedRolePool { get; private set; }

        public AdminLobbySession(ulong hostId, ulong channelId, LobbyMode mode)
            : base(hostId, channelId, mode)
        {
            AttachedRoles = new Dictionary<ulong, RoleVisual>();
            PreservedRolePool = new List<RoleVisual>();
        }

        public void PreserveRolePool()
        {
            foreach(KeyValuePair<RoleVisual, int> kvp in SelectedRolesPool)
            {
                for(int i = 0; i < kvp.Value; i++)
                {
                    PreservedRolePool.Add(kvp.Key);
                }
            }
        }
    }
}