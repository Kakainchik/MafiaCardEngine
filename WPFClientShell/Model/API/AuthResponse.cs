namespace WPFClientShell.Model.API
{
    public class AuthResponse
    {
        public long Id { get; init; }
        public string Username { get; init; }
        public string JwtToken { get; init; }

        public AuthResponse(long id, string username, string jwtToken)
        {
            Id = id;
            Username = username;
            JwtToken = jwtToken;
        }
    }
}