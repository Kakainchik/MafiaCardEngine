namespace WebServer.Model.User
{
    public interface ITokenRepository
    {
        Task<bool> IsTokenExist(string expectedToken);
        Task AttachTokenToUser(long userId, RefreshTokenEntity refreshToken);
        Task RemoveUserOldTokens(long userId);
    }
}