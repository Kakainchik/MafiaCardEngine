using System.ComponentModel.DataAnnotations;

namespace WebServer.Model.Room
{
    public record class CreateRoomDTO
    {
        [MinLength(1)]
        public string Title { get; init; }

        [MaxLength(50)]
        public string? Description { get; init; }

        [Range(5, 50)]
        public int MaxSeats { get; init; }

        public CreateRoomDTO(string title, string? description, int maxSeats)
        {
            Title = title;
            Description = description;
            MaxSeats = maxSeats;
        }
    }
}