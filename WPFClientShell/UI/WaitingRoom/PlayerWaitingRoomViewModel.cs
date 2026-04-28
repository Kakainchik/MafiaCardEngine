using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;
using WPFClientShell.Core;
using WPFClientShell.Extensions;
using WPFClientShell.Model.API;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.UI
{
    public class PlayerWaitingRoomViewModel : BaseViewModel, IAsyncNavigationService, IParameterNavigationService
    {
        private readonly ViewModelRouter router;
        private readonly LobbyDomain lobbyDomain;
        private readonly AuthSettings authSettings;

        private LobbyEntity currentLobbyData = null!;
        private string chatText = string.Empty;
        private bool isReady = false;

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

        public bool IsReady
        {
            get => isReady;
            set
            {
                isReady = value;
                OnPropertyChanged(nameof(IsReady));
            }
        }

        public ObservableCollection<ChatMessage> ChatLines { get; set; }

        public ICommand NavigateBackCommand { get; set; }
        public ICommand DropMessageCommand { get; set; }
        public ICommand ReadyCommand { get; set; }

        public PlayerWaitingRoomViewModel(ViewModelRouter router, LobbyDomain lobbyDomain, IOptionsMonitor<AuthSettings> authSettings)
        {
            this.router = router;
            this.lobbyDomain = lobbyDomain;
            this.authSettings = authSettings.CurrentValue;

            ChatLines = new ObservableCollection<ChatMessage>();

            NavigateBackCommand = new RelayCommand(OnNavigateBack);
            DropMessageCommand = new RelayCommand(OnDropMessage);
            ReadyCommand = new RelayCommand(OnReady);
        }

        public void ParameterInitialize(params object[] parameters)
        {
            currentLobbyData = (LobbyEntity)parameters[0];
        }

        public async Task InitializeAsync()
        {
            try
            {
                lobbyDomain.ContextReceived += LobbyDomain_ContextReceived;
                lobbyDomain.ChatContextReceived += LobbyDomain_ChatContextReceived;

                WaitingRoomContext roomContext = await lobbyDomain.JoinLobbyAsync(CurrentLobbyData.Id, App.Current.Dispatcher);

                //Update waiting room details
                CurrentLobbyData.MaxSeats = roomContext.MaxSeats;

                CurrentLobbyData.Players.Clear();
                for(int i = 0; i < roomContext.Players.Length; i++)
                {
                    WaitingRoomContext.WaitingRoomUser roomUser = roomContext.Players[i];
                    UserReadinessDecorator decorator = new UserReadinessDecorator(roomUser.Id,
                        roomUser.Username,
                        roomUser.IsReady);
                    CurrentLobbyData.Players.Add(decorator);
                }

                CurrentLobbyData.Roles.Clear();
                foreach(KeyValuePair<RoleSignature, int> s in roomContext.Roles)
                {
                    CurrentLobbyData.Roles[s.Key.MapRole()] = s.Value;
                }
            }
            catch(Exception)
            {
                await lobbyDomain.LeaveLobbyAsync();

                router.NavigateTo(nameof(HallView));
            }
        }
        
        private async void OnNavigateBack(object? obj)
        {
            IsBusy = true;

            await lobbyDomain.LeaveLobbyAsync();

            router.NavigateTo(nameof(HallView));

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

        private async void OnReady(object? obj)
        {
            IsBusy = true;

            IsReady = !IsReady;

            WaitingRoomReadyChangeContext context = new WaitingRoomReadyChangeContext
            {
                UserId = authSettings.UserId,
                IsReady = this.IsReady
            };
            await lobbyDomain.SendContextAsync(context);

            IsBusy = false;
        }

        private async void LobbyDomain_ContextReceived(Context obj)
        {
            switch(obj)
            {
                case KickPlayerContext kpc:
                {
                    //We have been kicked
                    IsBusy = true;
                    BusyReason = "You have been kicked!";
                    
                    await lobbyDomain.LeaveLobbyAsync();
                    await Task.Delay(3000);

                    router.NavigateTo(nameof(MainMenuView));

                    IsBusy = false;
                    break;
                }
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
                case WaitingRoomRoleChangeContext wrrcc:
                {
                    //Role set has been changed
                    if(wrrcc.Amount == 0)
                    {
                        CurrentLobbyData.Roles.Remove(wrrcc.Role.MapRole());
                    }
                    else
                    {
                        CurrentLobbyData.Roles[wrrcc.Role.MapRole()] = wrrcc.Amount;
                    }
                    break;
                }
                case WaitingRoomSeatsChangeContext wrscc:
                {
                    //MaxSeats has been changed
                    CurrentLobbyData.MaxSeats = wrscc.MaxSeats;
                    break;
                }
                case GameReadyContext grc:
                {
                    IsBusy = true;

                    lobbyDomain.ContextReceived -= LobbyDomain_ContextReceived;
                    lobbyDomain.ChatContextReceived -= LobbyDomain_ChatContextReceived;

                    //Game initialized
                    await router.NavigateToAsync(nameof(GamePanelView));

                    IsBusy = false;
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