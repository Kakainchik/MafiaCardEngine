using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Windows;
using System.Windows.Threading;
using WPFClientShell.Core;
using WPFClientShell.DataSource;
using WPFClientShell.Model.API;
using WPFClientShell.Model.Hub;
using WPFClientShell.Properties;
using WPFClientShell.UI;

namespace WPFClientShell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider serviceProvider = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            serviceProvider = serviceCollection.BuildServiceProvider();

            MainWindow = serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            IOptionsMonitor<AuthSettings> monitor = serviceProvider.GetRequiredService<IOptionsMonitor<AuthSettings>>();

            Settings.Default.UserId = monitor.CurrentValue.UserId;
            Settings.Default.Username = monitor.CurrentValue.Username;
            Settings.Default.JWT = monitor.CurrentValue.JWT;
            Settings.Default.RefreshToken = monitor.CurrentValue.RefreshToken;
            Settings.Default.Save();

#if DEBUG
            Settings.Default.Upgrade();
#endif

            // Dispose of services if needed
            if(serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        private static void ConfigureRouting(IServiceCollection services)
        {
            services.AddSingleton<ViewModelRouter>(sp =>
            {
                return new ViewModelRouter(new Dictionary<string, INavigationService>
                {
                    {
                        nameof(MainMenuView),
                        new NavigationService<MainMenuViewModel>(sp.GetRequiredService<NavigationStore>(),
                            () => sp.GetRequiredService<MainMenuViewModel>())
                    },
                    {
                        nameof(HallView),
                        new NavigationService<HallViewModel>(sp.GetRequiredService<NavigationStore>(),
                            () => sp.GetRequiredService<HallViewModel>())
                    },
                    {
                        nameof(CreateLobbyView),
                        new NavigationService<CreateLobbyViewModel>(sp.GetRequiredService<NavigationStore>(),
                            () => sp.GetRequiredService<CreateLobbyViewModel>())
                    },
                    {
                        nameof(HostWaitingRoomView),
                        new NavigationService<HostWaitingRoomViewModel>(sp.GetRequiredService<NavigationStore>(),
                            () => sp.GetRequiredService<HostWaitingRoomViewModel>())
                    },
                    {
                        nameof(PlayerWaitingRoomView),
                        new NavigationService<PlayerWaitingRoomViewModel>(sp.GetRequiredService<NavigationStore>(),
                            () => sp.GetRequiredService<PlayerWaitingRoomViewModel>())
                    },
                    {
                        nameof(GamePanelView),
                        new NavigationService<GamePanelViewModel>(sp.GetRequiredService<NavigationStore>(),
                            () => sp.GetRequiredService<GamePanelViewModel>())
                    }
                });
            });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            Settings settings = Settings.Default;
            services.Configure<AuthSettings>(s =>
            {
                s.UserId = settings.UserId;
                s.Username = settings.Username;
                s.RefreshToken = settings.RefreshToken;
                s.JWT = settings.JWT;
            });

            // Configure Logging
            services.AddLogging();

            // Register Views
            services.AddSingleton<MainWindow>();

            // Register ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainMenuViewModel>();
            services.AddSingleton<HallViewModel>();
            services.AddTransient<CreateLobbyViewModel>();
            services.AddTransient<HostWaitingRoomViewModel>();
            services.AddTransient<PlayerWaitingRoomViewModel>();
            services.AddTransient<GamePanelViewModel>();

            // Register Services
            services.AddSingleton<NavigationStore>();

#if DEBUG
            services.AddSingleton<ApiDataSource>(sp => new ApiDataSource("http://localhost:5066/api/"));
            services.AddSingleton<LobbyHubDataSource>(sp =>
            {
                IOptionsMonitor<AuthSettings> monitor = sp.GetRequiredService<IOptionsMonitor<AuthSettings>>();
                return new LobbyHubDataSource("http://localhost:5066/hub/", monitor.CurrentValue.JWT);
            });
#else
            services.AddSingleton<ApiDataSource>(sp => new ApiDataSource(Settings.Default.BaseApiUrl));
            services.AddSingleton<LobbyHubDataSource>(sp =>
            {
                IOptionsMonitor<AuthSettings> monitor = sp.GetRequiredService<IOptionsMonitor<AuthSettings>>();
                return new LobbyHubDataSource(Settings.Default.BaseHubUrl, monitor.CurrentValue.JWT);
            });
#endif
            services.AddSingleton<AuthDomain>();
            services.AddSingleton<HallDomain>();
            services.AddSingleton<LobbyDomain>();

            ConfigureRouting(services);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}