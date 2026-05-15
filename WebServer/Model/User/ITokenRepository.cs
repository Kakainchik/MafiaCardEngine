namespace WebServer.Model.User
{
    public interface ITokenRepository
    {
        Task<bool> IsTokenExist(string expectedToken);
        Task AttachTokenToUser(ulong userId, RefreshTokenEntity refreshToken);
        Task RemoveUserOldTokens(ulong userId);
    }
}