using Discord.WebSocket;
using GameLogic.ParanoiaCorp;

namespace DiscordBot.Model
{
    public class GameHolder
    {
        public object LockObject { get; }
        public SocketGuild Guild { get; }
        public GameEngine Engine { get; }

        public GameHolder(SocketGuild guild, GameEngine engine)
        {
            LockObject = new object();
            Guild = guild;
            Engine = engine;
        }
    }
}