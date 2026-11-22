using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using WebServer.Shared.GameObjects;
using ParanoiaRoleSignature = WebServer.Shared.ParanoiaCorp.GameObjects.RoleSignature;

namespace WebServer.Model.Room
{
    public class WaitRoomDomain
    {
        private static int sharedId = 1;

        public int Id { get; } = ++sharedId;

        public UserRoomDomain Host { get; }
        public string Title { get; }
        public DateTime Created { get; }
        public DateTime Expires { get; }
        public IDictionary<ulong, UserRoomDomain> Players { get; }
        public IDictionary<RoleSignature, int> Roles { get; }
        public IDictionary<ParanoiaRoleSignature, int> ParanoiaRoles { get; }

        public string? Description { get; set; }

        [Range(2, 50)]
        public int MaxSeats { get; set; }

        public int Fullness => Players.Count;
        public bool IsFull => Fullness == MaxSeats;
        public bool IsValid => Expires > DateTime.UtcNow;
        public bool IsHostJoined => Players.ContainsKey(Host);

        public WaitRoomDomain(UserRoomDomain host, string title, string? description, int maxSeats)
        {
            Host = host;
            Title = title;
            Description = description;
            MaxSeats = maxSeats;

            Created = DateTime.UtcNow;
            Expires = DateTime.UtcNow.AddHours(2);
            Players = new ConcurrentDictionary<ulong, UserRoomDomain>();
            Roles = new ConcurrentDictionary<RoleSignature, int>();
            ParanoiaRoles = new ConcurrentDictionary<ParanoiaRoleSignature, int>();

            // General Director is mandatory for Paranoia Corp
            ParanoiaRoles[ParanoiaRoleSignature.GENERAL_DIRECTOR] = 1;
        }
    }
}
