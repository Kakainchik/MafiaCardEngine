using WebServer.Model.User;

namespace WebServer.Model.Room
{
    public record class WaitRoomDTO
    {
        public int Id { get; init; }
        public UserDTO Host { get; init; }
        public string Title { get; init; }
        public string? Description { get; init; }
        public int MaxSeats { get; init; }
        public int Fullness { get; init; }

        public bool IsFull => Fullness == MaxSeats;

        public WaitRoomDTO(int id, UserDTO host, string title, string? description, int fullness, int maxSeats)
        {
            Id = id;
            Host = host;
            Title = title;
            Description = description;
            Fullness = fullness;
            MaxSeats = maxSeats;
        }
    }
}