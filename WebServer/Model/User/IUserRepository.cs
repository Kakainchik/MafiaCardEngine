namespace WebServer.Model.User
{
    public interface IUserRepository
    {
        ValueTask<UserEntity?> GetUser(long id);
        Task<UserEntity?> GetUser(string username);
        Task<UserEntity?> GetUserByRefreshToken(string refreshToken);
        Task<IEnumerable<UserEntity>> GetUsers(Range range);
        Task<bool> AddUser(UserEntity user);
        Task<bool> IsUserExist(string username);
        Task UpdateUser(UserEntity user);
    }
}