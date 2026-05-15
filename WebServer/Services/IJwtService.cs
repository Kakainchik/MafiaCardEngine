using WebServer.Model.User;

namespace WebServer.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(ulong userId, string Username);
        Task<RefreshTokenDTO> GenerateRefreshToken();
        int? ValidateJwtToken(string token);
    }
}