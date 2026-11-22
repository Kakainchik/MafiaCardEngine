using System.Collections.Concurrent;

namespace WebServer.Model.ParanoiaCorp
{
    public interface IParanoiaGameRepository
    {
        void AddGame(int roomId, ParanoiaGameSession session);
        ParanoiaGameSession? GetGame(int roomId);
        bool HasGame(int roomId);
        bool RemoveGame(int roomId);
    }

    public class ParanoiaGameRepository : IParanoiaGameRepository
    {
        private readonly ConcurrentDictionary<int, ParanoiaGameSession> activeGames = new();

        public void AddGame(int roomId, ParanoiaGameSession session)
        {
            activeGames[roomId] = session;
        }

        public ParanoiaGameSession? GetGame(int roomId)
        {
            activeGames.TryGetValue(roomId, out var session);
            return session;
        }

        public bool HasGame(int roomId)
        {
            return activeGames.ContainsKey(roomId);
        }

        public bool RemoveGame(int roomId)
        {
            return activeGames.TryRemove(roomId, out _);
        }
    }
}
