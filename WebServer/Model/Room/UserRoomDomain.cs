namespace WebServer.Model.Room
{
    public class UserRoomDomain
    {
        public ulong Id { get; init; }
        public string Username { get; init; }
        public bool IsReady { get; set; }

        public UserRoomDomain(ulong id, string username)
        {
            Id = id;
            Username = username;
        }

        public override bool Equals(object? obj)
        {
            if(obj is ulong)
            {
                return (ulong)obj == Id;
            }

            UserRoomDomain? second = obj as UserRoomDomain;
            if(second is null)
            {
                return false;
            }
            else
            {
                return second.Id == Id;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static implicit operator ulong(UserRoomDomain dto) => dto.Id;
    }
}