using Discord.WebSocket;
using GameLogic.ParanoiaCorp;

namespace DiscordBot.Model
{
    public class GameHolder
    {
        public SocketGuild Guild { get; }
        public GameEngine Engine { get; }

        public GameHolder(SocketGuild guild, GameEngine engine)
        {
            Guild = guild;
            Engine = engine;
        }
    }
}