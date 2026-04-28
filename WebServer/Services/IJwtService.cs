using WebServer.Model.User;

namespace WebServer.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(long userId, string Username);
        Task<RefreshTokenDTO> GenerateRefreshToken();
        int? ValidateJwtToken(string token);
    }
}