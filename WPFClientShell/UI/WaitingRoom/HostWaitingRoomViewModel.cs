using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WebServer.Shared.HubObjects;
using WPFClientShell.Core;
using WPFClientShell.Extensions;
using WPFClientShell.Model.API;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.UI
{
    public class HostWaitingRoomViewModel : BaseViewModel, IAsyncNavigationService, IParameterNavigationService
    {
        private readonly ViewModelRouter router;
        private readonly LobbyDomain lobbyDomain;
        private readonly AuthSettings authSettings;

        private LobbyEntity currentLobbyData = null!;
        private string chatText = string.Empty;

        public LobbyEntity CurrentLobbyData
        {
            get => currentLobbyData;
            set
            {
                currentLobbyData = value;
                OnPropertyChanged(nameof(CurrentLobbyData));
            }
        }

        public string ChatText
        {
            get => chatText;
            set
            {
                chatText = value;
                OnPropertyChanged(nameof(ChatText));
            }
        }

        public ObservableCollection<ChatMessage> ChatLines { get; set; }

        public ICommand UpMaxSeatsCommand { get; set; }
        public ICommand DownMaxSeatsCommand { get; set; }
        public ICommand NavigateBackCommand { get; set; }
        public ICommand RoleUpdateCommand { get; set; }
        public ICommand PlayerKickedCommand { get; set; }
        public ICommand DropMessageCommand { get; set; }
        public ICommand DoneCommand { get; set; }

        public HostWaitingRoomViewModel(ViewModelRouter router, LobbyDomain lobbyDomain, IOptionsMonitor<AuthSettings> authSettings)
        {
            this.router = router;
            this.lobbyDomain = lobbyDomain;
            this.authSettings = authSettings.CurrentValue;

            ChatLines = new ObservableCollection<ChatMessage>();

            UpMaxSeatsCommand = new RelayCommand(OnUpMaxSeats);
            DownMaxSeatsCommand = new RelayCommand(OnDownMaxSeats);
            NavigateBackCommand = new RelayCommand(OnNavigateBack);
            RoleUpdateCommand = new RelayCommand(OnRoleUpdate);
            PlayerKickedCommand = new RelayCommand(OnPlayerKicked);
            DropMessageCommand = new RelayCommand(OnDropMessage);
            DoneCommand = new RelayCommand(OnDone);
        }

        public void ParameterInitialize(params object[] parameters)
        {
            currentLobbyData = (LobbyEntity)parameters[0];
        }

        public async Task InitializeAsync()
        {
            try
            {
                await lobbyDomain.JoinLobbyAsync(CurrentLobbyData.Id, App.Current.Dispatcher);

                lobbyDomain.ContextReceived += LobbyDomain_ContextReceived;
                lobbyDomain.ChatContextReceived += LobbyDomain_ChatContextReceived;

                //Add host
                UserReadinessDecorator host = new UserReadinessDecorator(CurrentLobbyData.Host.Id,
                    CurrentLobbyData.Host.Username)
                {
                    IsReady = true
                };
                CurrentLobbyData.Players.Add(host);
            }
            catch(Exception)
            {
                await lobbyDomain.LeaveLobbyAsync();

                router.NavigateTo(nameof(HallView));
            }
        }

        private async void OnUpMaxSeats(object? obj)
        {
            if(CurrentLobbyData.MaxSeats == 50)
            {
                return;
            }
            CurrentLobbyData.MaxSeats++;

            IsBusy = false;

            WaitingRoomSeatsChangeContext context = new WaitingRoomSeatsChangeContext
            {
                MaxSeats = CurrentLobbyData.MaxSeats
            };
            await lobbyDomain.SendContextAsync(context);

            IsBusy = false;
        }

        private async void OnDownMaxSeats(object? obj)
        {
            if(CurrentLobbyData.MaxSeats == 5 || CurrentLobbyData.MaxSeats == CurrentLobbyData.Players.Count)
            {
                return;
            }
            CurrentLobbyData.MaxSeats--;

            IsBusy = true;

            WaitingRoomSeatsChangeContext context = new WaitingRoomSeatsChangeContext
            {
                MaxSeats = CurrentLobbyData.MaxSeats
            };
            await lobbyDomain.SendContextAsync(context);

            IsBusy = false;
        }

        private async void OnNavigateBack(object? obj)
        {
            IsBusy = true;

            await lobbyDomain.LeaveLobbyAsync();

            router.NavigateTo(nameof(HallView));

            IsBusy = false;
        }

        private async void OnRoleUpdate(object? obj)
        {
            RoleVisual role = (RoleVisual)obj!;

            IsBusy = true;

            int amount = 0;
            if(CurrentLobbyData.Roles.ContainsKey(role))
            {
                amount = CurrentLobbyData.Roles[role];
            }

            WaitingRoomRoleChangeContext context = new WaitingRoomRoleChangeContext
            {
                Role = role.MapRole(),
                Amount = amount
            };
            await lobbyDomain.SendContextAsync(context);

            IsBusy = false;
        }

        private async void OnPlayerKicked(object? obj)
        {
            ulong playerId = (ulong)obj!;
            if(CurrentLobbyData.Host.Id == playerId)
            {
                return;
            }

            IsBusy = true;

            KickPlayerContext context = new KickPlayerContext
            {
                UserToKickId = playerId
            };
            await lobbyDomain.SendContextAsync(context);

            IsBusy = false;
        }

        private async void OnDropMessage(object? obj)
        {
            ChatMessage msg = new ChatMessage(authSettings.Username!, ChatText, TimeOnly.FromDateTime(DateTime.Now));
            ChatLines.Add(msg);

            ChatContext context = new ChatContext(msg.Who, msg.Message);
            await lobbyDomain.SendChatContextAsync(context);

            ChatText = string.Empty;
        }

        private async void OnDone(object? obj)
        {
            IsBusy = true;

            //Check if are able to run the game
            int rolesAmount = CurrentLobbyData.Roles.Sum(s => s.Value);
            if(CurrentLobbyData.Players.Count != rolesAmount)
            {
                MessageBox.Show("Amount of roles and players do not equal.",
                    "Unable to run game!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if(CurrentLobbyData.Players.Count < 5)
            {
                MessageBox.Show("Minimum players must be 5.",
                    "Unable to run game!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            ChatLines.Clear();

            ServerRequestContext request = new ServerRequestContext
            {
                RequestFor = ServerRequestType.FOR_INITIALIZE_GAME
            };
            GameReadyContext response = await lobbyDomain.SendServerRequestAsync<GameReadyContext>(request);

            if(response.Success)
            {
                await router.NavigateToAsync(nameof(GamePanelView));
            }

            IsBusy = false;
        }

        private void LobbyDomain_ContextReceived(Context obj)
        {
            switch(obj)
            {
                case UserAbsenceContext uac:
                {
                    //User has appeared or left
                    UserReadinessDecorator decorator = new UserReadinessDecorator(uac.UserId, uac.Username);
                    if(uac.HasAdded)
                    {
                        CurrentLobbyData.Players.Add(decorator);
                    }
                    else if(uac.HasRemoved)
                    {
                        CurrentLobbyData.Players.Remove(decorator);
                    }
                    break;
                }
                case WaitingRoomReadyChangeContext wrrcc:
                {
                    //Readiness has been changed
                    CurrentLobbyData.Players.Single(p => p.Id == wrrcc.UserId).IsReady = wrrcc.IsReady;
                    break;
                }
            }
        }

        private void LobbyDomain_ChatContextReceived(ChatContext obj)
        {
            ChatMessage msg = new ChatMessage(obj.Who, obj.Message, TimeOnly.FromDateTime(DateTime.Now));
            ChatLines.Add(msg);
        }
    }
}