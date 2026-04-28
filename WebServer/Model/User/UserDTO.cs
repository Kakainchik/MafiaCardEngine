namespace WebServer.Model.User
{
    public class UserDTO
    {
        public long Id { get; init; }
        public string Username { get; init; }

        public UserDTO(long id, string username)
        {
            Id = id;
            Username = username;
        }

        public override bool Equals(object? obj)
        {
            UserDTO? second = obj as UserDTO;
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

        public static implicit operator long(UserDTO dto) => dto.Id;

        public static explicit operator UserDTO(long id) => new UserDTO(id, string.Empty);
    }
}