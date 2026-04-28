using Microsoft.EntityFrameworkCore;

namespace WebServer.Model.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthContext db;

        public UserRepository(AuthContext context)
        {
            this.db = context;
        }

        public async ValueTask<UserEntity?> GetUser(long id)
        {
            return await db.Users.FindAsync(id);
        }

        public async Task<UserEntity?> GetUser(string username)
        {
            return await db.Users.SingleOrDefaultAsync(u => u.Username.Equals(username));
        }

        public async Task<UserEntity?> GetUserByRefreshToken(string refreshToken)
        {
            return await db.Users.SingleOrDefaultAsync<UserEntity>(u => u.RefreshTokens.Any(t => t.Token.Equals(refreshToken)));
        }

        public async Task<IEnumerable<UserEntity>> GetUsers(Range range)
        {
            int position = range.Start.Value;
            int limit = range.End.Value - range.Start.Value;
            return await db.Users.Skip(position)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> IsUserExist(string username)
        {
            return await db.Users.AnyAsync(user => user.Username.Equals(username));
        }

        public async Task<bool> AddUser(UserEntity user)
        {
            await db.Users.AddAsync(user);
            return await db.SaveChangesAsync() == 1;
        }

        public async Task UpdateUser(UserEntity user)
        {
            db.Update<UserEntity>(user);
            await db.SaveChangesAsync();
        }
    }
}