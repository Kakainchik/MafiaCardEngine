using WPFClientShell.Core;
using WPFClientShell.UI;

namespace WPFClientShell
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly NavigationStore navigationStore;

        private BaseViewModel currentViewModel = null!;

        public BaseViewModel CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public MainWindowViewModel(NavigationStore navigationStore, MainMenuViewModel mainMenuVm)
        {
            this.navigationStore = navigationStore;

            CurrentViewModel = this.navigationStore.CurrentViewModel = mainMenuVm;

            this.navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            if(navigationStore.CurrentViewModel is null)
            {
                throw new ArgumentNullException(nameof(navigationStore.CurrentViewModel), "No view-model provided.");
            }

            CurrentViewModel = navigationStore.CurrentViewModel;
        }
    }
}