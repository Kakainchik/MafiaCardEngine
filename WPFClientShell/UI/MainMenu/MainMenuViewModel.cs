using System.Windows.Input;
using WPFClientShell.Core;
using WPFClientShell.Model.API;

namespace WPFClientShell.UI
{
    public class MainMenuViewModel : BaseViewModel
    {
        private const string AUTHENTICATED_MESSAGE = "You are authenticated!";
        private const string NOT_AUTHENTICATED_MESSAGE = "You are not authenticated.";
        private const string SIGN_IN_FAIL_MESSAGE = "Unable to sign in. Username or password incorrect.";
        private const string SIGN_UP_FAIL_MESSAGE = "Unable to sign up. Probably such a user exists.";
        private const string SIGNED_UP_MESSAGE = "You have signed up.";

        private readonly ViewModelRouter router;
        private readonly AuthDomain authDomain;

        private bool isAuthenticated;
        private string username = string.Empty;
        private string password = string.Empty;
        private string authInfo = string.Empty;
        private string nickname = string.Empty;
        private string serverAddress = string.Empty;

        public bool IsAuthenticated
        {
            get => isAuthenticated;
            set
            {
                isAuthenticated = value;
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        }

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string AuthInfo
        {
            get => authInfo;
            set
            {
                authInfo = value;
                OnPropertyChanged(nameof(AuthInfo));
            }
        }

        public string Nickname
        {
            get => nickname;
            set
            {
                nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public ICommand OpenHallCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }

        public MainMenuViewModel(ViewModelRouter router, AuthDomain authDomain)
        {
            this.router = router;
            this.authDomain = authDomain;

            IsAuthenticated = authDomain.IsAuthenticated;
            if(IsAuthenticated)
            {
                AuthInfo = AUTHENTICATED_MESSAGE;
                Nickname = authDomain.Username!;
            }
            else
            {
                AuthInfo = NOT_AUTHENTICATED_MESSAGE;
            }

            OpenHallCommand = new RelayCommand(OnOpenHall);
            LoginCommand = new RelayCommand(OnLogin);
            RegisterCommand = new RelayCommand(OnRegister);
        }

        private async void OnOpenHall(object? obj)
        {
            IsBusy = true;

            await router.NavigateToAsync(nameof(HallView));

            IsBusy = false;
        }

        private async void OnLogin(object? obj)
        {
            IsBusy = true;

            bool success = await authDomain.Authenticate(Username, Password);
            if(success)
            {
                AuthInfo = AUTHENTICATED_MESSAGE;
                Nickname = authDomain.Username!;
            }
            else
            {
                AuthInfo = SIGN_IN_FAIL_MESSAGE;
            }

            IsAuthenticated = success;
            IsBusy = false;
        }

        private async void OnRegister(object? obj)
        {
            IsBusy = true;

            bool success = await authDomain.Register(Username, Password);
            if(success)
            {
                AuthInfo = SIGNED_UP_MESSAGE;
            }
            else
            {
                AuthInfo = SIGN_UP_FAIL_MESSAGE;
            }

            IsBusy = false;
        }
    }
}