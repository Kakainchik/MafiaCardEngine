namespace WebServer.Model.User
{
    public record class AuthenticateUserDTO
    {
        public string Username { get; init; }
        public string Password { get; init; }

        public AuthenticateUserDTO(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}