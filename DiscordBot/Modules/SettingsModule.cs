using Discord.Interactions;
using DiscordBot.Resources;
using System.Collections.Concurrent;
using System.Globalization;

namespace DiscordBot.Modules
{
    public class SettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        public static ConcurrentDictionary<ulong, CultureInfo> UserPreferences { get; } = new();

        [SlashCommand("language", "Set your preferred language")]
        public async Task SetLanguage([Summary("language")] string language)
        {
            CultureInfo cultureInfo = new CultureInfo(language, true);
            UserPreferences[Context.Guild.Id] = cultureInfo;

            await RespondAsync($"Language set to {language}.", ephemeral: true);
        }
    }
}