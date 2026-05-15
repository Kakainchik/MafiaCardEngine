using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Modules;
using System.Reflection;

namespace DiscordBot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient client;
        private readonly InteractionService commands;
        private readonly IServiceProvider services;

        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            this.client = client;
            this.commands = commands;
            this.services = services;
        }

        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            client.InteractionCreated += HandleInteractionAsync;
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                if(interaction.GuildId.HasValue && SettingsModule.UserPreferences.ContainsKey(interaction.GuildId.Value))
                {
                    Thread.CurrentThread.CurrentUICulture = SettingsModule.UserPreferences[interaction.GuildId.Value];
                }
                
                SocketInteractionContext context = new SocketInteractionContext(client, interaction);
                await commands.ExecuteCommandAsync(context, services);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}