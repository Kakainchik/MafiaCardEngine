using System.Collections.Concurrent;
using WebServer.Shared.GameObjects;

namespace WebServer.Model.Room
{
    public class RoomRepository : IWaitRoomRepository
    {
        //In-memory data
        private readonly ConcurrentDictionary<int, WaitRoomDomain> rooms;

        public RoomRepository()
        {
            rooms = new ConcurrentDictionary<int, WaitRoomDomain>();
        }

        public IEnumerable<WaitRoomDomain> GetRooms(Range range)
        {
            return rooms.Values.Take(range);
        }

        public IEnumerable<WaitRoomDomain> GetValidRooms(Range range)
        {
            return rooms.Values.Where(l => l.IsValid).Take(range);
        }

        public WaitRoomDomain? GetRoom(int id)
        {
            WaitRoomDomain? domain;
            rooms.TryGetValue(id, out domain);
            return domain;
        }

        public bool IsRoomExist(int roomId)
        {
            return rooms.ContainsKey(roomId);
        }

        public int? GetRoomIdByPlayer(ulong userId)
        {
            return rooms.Values.SingleOrDefault(l => l.Players.Keys.Any(u => u == userId))?.Id;
        }

        public bool AddRoom(WaitRoomDomain domain)
        {
            if(!ValidateNewLobby(domain))
            {
                return false;
            }

            rooms[domain.Id] = domain;

            return true;
        }

        public bool IsPlayerExist(ulong userId)
        {
            return rooms.Any(l => l.Value.Players.Keys.Any(u => u == userId));
        }

        public bool IsPlayerExist(int roomId, ulong userId)
        {
            WaitRoomDomain? domain;
            if(rooms.TryGetValue(roomId, out domain))
            {
                return domain.Players.Keys.Any(u => u == userId);
            }
            return false;
        }

        public bool IsHostPresent(int roomId)
        {
            WaitRoomDomain? domain;
            if(rooms.TryGetValue(roomId, out domain))
            {
                return domain.IsHostJoined;
            }
            return false;
        }

        public void AddPlayerToRoom(int roomId, UserRoomDomain player)
        {
            WaitRoomDomain? domain = GetRoom(roomId);
            if(domain is null || IsPlayerExist(player.Id))
            {
                throw new InvalidOperationException("User dublication error.");
            }

            domain.Players.Add(player, player);
        }

        public void KickPlayerFromRoom(int roomId, ulong userId)
        {
            WaitRoomDomain? domain;
            if(rooms.TryGetValue(roomId, out domain))
            {
                UserRoomDomain? user;
                if(domain.Players.TryGetValue(userId, out user))
                {
                    domain.Players.Remove(user);
                }
            }
        }

        private bool ValidateNewLobby(WaitRoomDomain domain)
        {
            if(!domain.IsValid)
            {
                return false;
            }

            //A predicate that checks if host from new lobby is present elsewhere
            Func<WaitRoomDomain, bool> hostPresentPredicate = delegate (WaitRoomDomain existingLobby)
            {
                if(existingLobby.Host.Equals(domain.Host))
                {
                    //A lobby with such a host already exists
                    return true;
                }

                if(existingLobby.Players.ContainsKey(domain.Host))
                {
                    //Such a host already present in another lobby
                    return true;
                }

                return false;
            };

            IEnumerable<WaitRoomDomain> validLobbies = rooms.Values.Where(l => l.IsValid);

            if(validLobbies.Any(hostPresentPredicate))
            {
                return false;
            }

            return true;
        }

        public UserRoomDomain GetHost(int roomId)
        {
            return rooms[roomId].Host;
        }

        public IEnumerable<UserRoomDomain> GetPlayers(int roomId)
        {
            return rooms[roomId].Players.Values;
        }

        public void SetReadiness(int roomId, ulong playerId, bool value)
        {
            rooms[roomId].Players[playerId].IsReady = value;
        }

        public IReadOnlyDictionary<RoleSignature, int> GetRoles(int roomId)
        {
            return rooms[roomId].Roles.AsReadOnly();
        }

        public void SetRole(int roomId, RoleSignature role, int amount)
        {
            if(amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount of role cannot be less than zero.");
            }

            if(amount == 0)
            {
                rooms[roomId].Roles.Remove(role);
            }
            else
            {
                rooms[roomId].Roles[role] = amount;
            }
        }

        public void SetMaxSeats(int roomId, int max)
        {
            if(max >= rooms[roomId].Players.Count)
            {
                rooms[roomId].MaxSeats = max;
            }
        }

        public void DeleteRoom(int roomId)
        {
            rooms.Remove(roomId, out _);
        }
    }
}