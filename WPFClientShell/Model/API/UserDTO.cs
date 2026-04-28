namespace WPFClientShell.Model.API
{
    public record class UserDTO
    {
        public long Id { get; init; }
        public string Username { get; init; }

        public UserDTO(long id, string username)
        {
            Id = id;
            Username = username;
        }
    }
}