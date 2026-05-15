namespace WebServer.Model.User
{
    public record class JwtDTO
    {
        public ulong Id { get; init; }
        public string Username { get; init; }
        public string JwtToken { get; init; }
        public string RefreshToken { get; init; }

        public JwtDTO(ulong id, string username, string jwtToken, string refreshToken)
        {
            Id = id;
            Username = username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}