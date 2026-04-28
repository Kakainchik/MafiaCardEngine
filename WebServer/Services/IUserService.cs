using WebServer.Model.User;

namespace WebServer.Services
{
    public interface IUserService
    {
        Task<bool> CreateUser(CreateUserDTO dto);
        Task<UserDTO?> GetUser(long userId);
        Task<IEnumerable<UserDTO>> GetUsers(int page);
    }
}