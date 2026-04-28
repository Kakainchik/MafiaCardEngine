namespace WPFClientShell.Model.API
{
    public record class CreateLobbyDTO
    {
        public string Title { get; init; }

        public string? Description { get; init; }

        public int MaxSeats { get; init; }

        public CreateLobbyDTO(string title, string? description, int maxSeats = 5)
        {
            Title = title;
            Description = description;
            MaxSeats = maxSeats;
        }
    }
}