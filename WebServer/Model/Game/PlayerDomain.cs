using GameLogic;
using WebServer.Shared.GameObjects;

namespace WebServer.Model.Game
{
    public class PlayerDomain
    {
        public string Nickname { get; }
        public Player GameRepresentation { get; }
        public RGB NColor { get; }

        public bool StartedGame { get; set; }

        public ulong Id => GameRepresentation.Id;

        public PlayerDomain(string nickname, RGB ncolor, Player representation)
        {
            Nickname = nickname;
            NColor = ncolor;
            GameRepresentation = representation;
        }
    }
}