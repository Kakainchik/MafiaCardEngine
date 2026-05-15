namespace WPFClientShell.Model.API
{
    public record class UserDTO
    {
        public ulong Id { get; init; }
        public string Username { get; init; }

        public UserDTO(ulong id, string username)
        {
            Id = id;
            Username = username;
        }
    }
}