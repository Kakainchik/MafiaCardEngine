using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    private IConfigurationRoot config;

    public static async Task Main(string[] args)
    {
        await new Program().MainAsync(args);
    }

    private async Task MainAsync(string[] args)
    {
        config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("secrets.json")
            .Build();

        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        await RunAsync(host);
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        DiscordSocketConfig socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true,
            UseInteractionSnowflakeDate = false
        };
        DiscordSocketClient client = new DiscordSocketClient(socketConfig);

        CommandService commandSerivce = new CommandService();
        LoggingService loggingService = new LoggingService(client, commandSerivce);

        //Dependency Injection setup
        collection.AddSingleton(config);
        collection.AddSingleton(client);
        collection.AddSingleton(x => new InteractionService(client));
        collection.AddSingleton<InteractionHandler>();
        collection.AddSingleton(commandSerivce);
        collection.AddSingleton<LobbyService>();
        collection.AddSingleton<GameSessionService>();
        collection.AddSingleton<DiscordStageController>();
        collection.AddSingleton<DiscordEndGameController>();
        collection.AddSingleton(loggingService);
    }

    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        DiscordSocketClient client = provider.GetRequiredService<DiscordSocketClient>();
        InteractionService interactionService = provider.GetRequiredService<InteractionService>();
        InteractionHandler interactionHandler = provider.GetRequiredService<InteractionHandler>();
        await interactionHandler.InitializeAsync();
        IConfigurationRoot config = provider.GetRequiredService<IConfigurationRoot>();

        client.Ready += async () =>
        {
            Console.WriteLine("Bot ready!");
            await interactionService.RegisterCommandsToGuildAsync(ulong.Parse(config["paranoiaCorpGuildId"]!));
        };

        await client.LoginAsync(TokenType.Bot, config["tokens:discord"]);
        await client.StartAsync();

        await Task.Delay(-1); // Keep the program running
    }
}