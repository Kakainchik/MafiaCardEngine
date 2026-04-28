using System.Collections.ObjectModel;
using System.Windows.Input;
using WPFClientShell.Core;
using WPFClientShell.Model.API;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.UI
{
    public class HallViewModel : BaseViewModel, IAsyncNavigationService
    {
        private readonly ViewModelRouter router;
        private readonly HallDomain hallDomain;

        private int page = 1;

        public int Page
        {
            get => page;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, nameof(Page));

                page = value;
                OnPropertyChanged(nameof(Page));
            }
        }

        public ObservableCollection<LobbyDTO> Lobbies { get; set; }

        public ICommand NavigateToMenuCommand { get; set; }
        public ICommand OpenCreateLobbyCommand { get; set; }
        public ICommand RefreshPageCommand { get; set; }
        public ICommand JoinCommand { get; set; }

        public HallViewModel(ViewModelRouter router, HallDomain hallDomain)
        {
            this.router = router;
            this.hallDomain = hallDomain;

            Lobbies = new ObservableCollection<LobbyDTO>();

            NavigateToMenuCommand = new RelayCommand(OnNavigateToMenu);
            OpenCreateLobbyCommand = new RelayCommand(OnOpenCreateLobby);
            RefreshPageCommand = new RelayCommand(OnRefreshPage);
            JoinCommand = new RelayCommand(OnJoin);
        }

        public async Task InitializeAsync()
        {
            await ObtainListPage();
        }

        private void OnNavigateToMenu(object? obj)
        {
            Page = 1;
            Lobbies.Clear();

            router.NavigateTo(nameof(MainMenuView));
        }

        private void OnOpenCreateLobby(object? obj)
        {
            router.NavigateTo(nameof(CreateLobbyView));
        }

        private async void OnRefreshPage(object? obj)
        {
            //Set page to default as the list get refreshed
            Page = 1;
            Lobbies.Clear();
            await ObtainListPage();
        }

        private async void OnJoin(object? obj)
        {
            IsBusy = true;

            int id = (int)obj!;

            LobbyDTO dto = await hallDomain.GetLobby(id);
            UserEntity host = new UserEntity(dto.Host.Id, dto.Host.Username);
            LobbyEntity entity = new LobbyEntity(dto.Id,
                host,
                dto.Title,
                dto.Description,
                dto.MaxSeats);

            await router.NavigateToAsync(nameof(PlayerWaitingRoomView), entity);

            IsBusy = false;
        }

        private async Task ObtainListPage()
        {
            IEnumerable<LobbyDTO> freshLobbies = await hallDomain.GetLobbies(Page);
            
            foreach(LobbyDTO l in freshLobbies)
            {
                Lobbies.Add(l);
            }
        }
    }
}