namespace WebServer.Model.Room
{
    public class UserRoomDomain
    {
        public long Id { get; init; }
        public string Username { get; init; }
        public bool IsReady { get; set; }

        public UserRoomDomain(long id, string username)
        {
            Id = id;
            Username = username;
        }

        public override bool Equals(object? obj)
        {
            if(obj is long)
            {
                return (long)obj == Id;
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

        public static implicit operator long(UserRoomDomain dto) => dto.Id;
    }
}