namespace WPFClientShell.Model.API
{
    public record class AuthRequest
    {
        public string Username { get; init; }
        public string Password { get; init; }

        public AuthRequest(string username, string pasword)
        {
            Username = username;
            Password = pasword;
        }
    }
}