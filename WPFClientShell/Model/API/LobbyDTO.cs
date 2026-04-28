namespace WPFClientShell.Model.API
{
    public record class LobbyDTO
    {
        public int Id { get; init; }
        public UserDTO Host { get; init; }
        public string Title { get; init; }
        public string? Description { get; init; }
        public int MaxSeats { get; init; }
        public int Fullness { get; init; }
        public bool IsFull { get; init; }

        public LobbyDTO(int id, UserDTO host, string title, string? description, int fullness, int maxSeats, bool isFull)
        {
            Id = id;
            Host = host;
            Title = title;
            Description = description;
            Fullness = fullness;
            MaxSeats = maxSeats;
            IsFull = isFull;
        }
    }
}
