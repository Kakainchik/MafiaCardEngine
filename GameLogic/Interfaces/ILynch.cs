namespace GameLogic.Interfaces
{
    public interface ILynch : IRoleOwner, IIdentifiable
    {
        string? LastMessage { get; set; }

        void Kill();
    }
}