using WebServer.Model.User;

namespace WebServer.Services
{
    public interface ITokenService
    {
        Task<JwtDTO> Authenticate(AuthenticateUserDTO dto);
        Task<JwtDTO> RefreshToken(string token);
        Task RevokeToken(string token);
    }
}