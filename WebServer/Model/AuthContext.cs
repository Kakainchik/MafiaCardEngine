using Microsoft.EntityFrameworkCore;
using WebServer.Model.User;

namespace WebServer.Model
{
    public class AuthContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; } = null!;

        public AuthContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}