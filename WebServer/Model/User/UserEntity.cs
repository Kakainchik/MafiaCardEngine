using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebServer.Model.User
{
    [Index(nameof(Username), IsUnique = true)]
    public class UserEntity
    {
        [Key]
        public ulong Id { get; set; }

        public string Username { get; set; } = null!;

        public string Hash { get; set; } = null!;

        public List<RefreshTokenEntity> RefreshTokens { get; set; } = null!;
    }
}