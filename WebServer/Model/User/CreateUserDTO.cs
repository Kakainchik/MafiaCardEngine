using System.ComponentModel.DataAnnotations;

namespace WebServer.Model.User
{
    public record class CreateUserDTO
    {
        [StringLength(16, MinimumLength = 5)]
        public string Username { get; init; }

        public string Password { get; init; }

        public CreateUserDTO(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}