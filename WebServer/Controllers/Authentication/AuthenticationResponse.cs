using Newtonsoft.Json;

namespace WebServer.Controllers.Authentication
{
    public record class AuthenticationResponse
    {
        public long Id { get; init; }
        public string Username { get; init; }
        public string JwtToken { get; init; }

        [JsonIgnore]
        public string RefreshToken { get; init; }

        public AuthenticationResponse(long id, string username, string jwtToken, string refreshToken)
        {
            Id = id;
            Username = username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}