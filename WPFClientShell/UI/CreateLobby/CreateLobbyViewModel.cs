using System.Windows.Input;
using WPFClientShell.Core;
using WPFClientShell.Model.API;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.UI
{
    public class CreateLobbyViewModel : BaseViewModel
    {
        private readonly ViewModelRouter router;
        private readonly HallDomain hallDomain;

        private bool canCreateLobby;
        private string lobbyTitle = string.Empty;
        private string lobbyDescription = string.Empty;

        public bool CanCreateLobby
        {
            get => canCreateLobby;
            set
            {
                canCreateLobby = value;
                OnPropertyChanged(nameof(CanCreateLobby));
            }
        }

        public string LobbyTitle
        {
            get => lobbyTitle;
            set
            {
                lobbyTitle = value;
                OnPropertyChanged(nameof(LobbyTitle));

                if(lobbyTitle.Length > 1)
                {
                    CanCreateLobby = true;
                }
                else
                {
                    CanCreateLobby = false;
                }
            }
        }

        public string LobbyDescription
        {
            get => lobbyDescription;
            set
            {
                lobbyDescription = value;
                OnPropertyChanged(nameof(LobbyDescription));
            }
        }

        public ICommand PublishLobbyCommand { get; set; }
        public ICommand NavigateToHallCommand { get; set; }

        public CreateLobbyViewModel(ViewModelRouter router, HallDomain hallDomain)
        {
            this.router = router;
            this.hallDomain = hallDomain;

            PublishLobbyCommand = new RelayCommand(OnPublishLobby);
            NavigateToHallCommand = new RelayCommand(OnNavigateToHall);
        }

        private async void OnPublishLobby(object? obj)
        {
            IsBusy = true;

            CreateLobbyDTO request = new CreateLobbyDTO(LobbyTitle, LobbyDescription);

            LobbyDTO dto = await hallDomain.CreateLobby(request);
            UserEntity host = new UserEntity(dto.Host.Id, dto.Host.Username);
            LobbyEntity entity = new LobbyEntity(dto.Id,
                host,
                dto.Title,
                dto.Description,
                dto.MaxSeats);

            //Join right away as a host
            await router.NavigateToAsync(nameof(HostWaitingRoomView), entity);

            IsBusy = false;
        }

        private void OnNavigateToHall(object? obj)
        {
            router.NavigateTo(nameof(HallView));
        }
    }
}