using WebServer.Shared.GameObjects;
using ParanoiaRoleSignature = WebServer.Shared.ParanoiaCorp.GameObjects.RoleSignature;

namespace WebServer.Model.Room
{
    public interface IWaitRoomRepository
    {
        IEnumerable<WaitRoomDomain> GetRooms(Range range);
        IEnumerable<WaitRoomDomain> GetValidRooms(Range range);
        WaitRoomDomain? GetRoom(int id);
        bool IsRoomExist(int roomId);
        int? GetRoomIdByPlayer(ulong userId);
        bool AddRoom(WaitRoomDomain domain);
        bool IsPlayerExist(ulong userId);
        bool IsPlayerExist(int roomId, ulong userId);
        bool IsHostPresent(int roomId);
        void AddPlayerToRoom(int roomId, UserRoomDomain player);
        void KickPlayerFromRoom(int roomId, ulong userId);
        UserRoomDomain GetHost(int roomId);
        IEnumerable<UserRoomDomain> GetPlayers(int roomId);
        void SetReadiness(int roomId, ulong playerId, bool value);
        IReadOnlyDictionary<RoleSignature, int> GetRoles(int roomId);
        void SetRole(int roomId, RoleSignature role, int amount);
        IReadOnlyDictionary<ParanoiaRoleSignature, int> GetParanoiaRoles(int roomId);
        void SetParanoiaRole(int roomId, ParanoiaRoleSignature role, int amount);
        void ClearParanoiaRoles(int roomId);
        void SetMaxSeats(int roomId, int max);
        void DeleteRoom(int roomId);
    }
}
