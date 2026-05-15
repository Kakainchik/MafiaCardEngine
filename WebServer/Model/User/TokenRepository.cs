using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebServer.Model.User
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AuthContext db;
        private readonly JwtSettings jwtSettings;

        public TokenRepository(AuthContext context, IOptions<JwtSettings> jwtSettings)
        {
            this.db = context;
            this.jwtSettings = jwtSettings.Value;
        }

        public async Task<bool> IsTokenExist(string expectedToken)
        {
            IQueryable<string> tokens = db.Users.SelectMany(user => user.RefreshTokens.Select(rt => rt.Token));
            return await tokens.AnyAsync(containedToken => containedToken.Equals(expectedToken));
        }

        public async Task AttachTokenToUser(ulong userId, RefreshTokenEntity refreshToken)
        {
            UserEntity? user = await db.Users.FindAsync(userId);
            if(user != null)
            {
                user.RefreshTokens.Add(refreshToken);

                db.Update<UserEntity>(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveUserOldTokens(ulong userId)
        {
            UserEntity? user = await db.Users.FindAsync(userId);
            if(user != null)
            {
                user.RefreshTokens.RemoveAll(t => t.IsExpired
                && t.Created.AddDays(jwtSettings.RefreshTokenTTL) <= DateTime.UtcNow);

                db.Update<UserEntity>(user);
                await db.SaveChangesAsync();
            }
        }
    }
}