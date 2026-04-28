using System.Collections.Concurrent;

namespace WebServer.Model.Game
{
    public class GameRepository : IGameRepository
    {
        private readonly ConcurrentDictionary<int, GameHolder> games;

        public GameRepository()
        {
            games = new ConcurrentDictionary<int, GameHolder>();
        }

        public void AddGame(int id, GameHolder game)
        {
            games[id] = game;
        }

        public GameHolder GetGame(int id)
        {
            return games[id];
        }

        public void Update(int id, GameHolder holder)
        {
            games.AddOrUpdate(id, holder, (fid, factory) =>
            {
                return holder;
            });
        }

        public bool Delete(int id)
        {
            return games.TryRemove(id, out _);
        }
    }
}