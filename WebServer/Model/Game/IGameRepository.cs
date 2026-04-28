namespace WebServer.Model.Game
{
    public interface IGameRepository
    {
        void AddGame(int id, GameHolder game);
        GameHolder GetGame(int id);
        void Update(int id, GameHolder holder);
        bool Delete(int id);
    }
}