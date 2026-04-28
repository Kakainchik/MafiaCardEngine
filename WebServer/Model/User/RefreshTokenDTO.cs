using System.Text.Json.Serialization;

namespace WebServer.Model.User
{
    public record class RefreshTokenDTO
    {
        public string RefreshToken { get; init; }

        public DateTime Created { get; init; }

        public DateTime Experies { get; init; }

        public RefreshTokenDTO(string refreshToken, DateTime created, DateTime expires)
        {
            RefreshToken = refreshToken;
            Created = created;
            Experies = expires;
        }
    }
}