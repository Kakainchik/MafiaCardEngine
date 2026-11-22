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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }
    }
}
