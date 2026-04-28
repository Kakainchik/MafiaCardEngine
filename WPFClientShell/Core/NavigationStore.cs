namespace WPFClientShell.Core
{
    public class NavigationStore
    {
        private BaseViewModel? currentViewModel;

        public BaseViewModel? CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public event Action? CurrentViewModelChanged;

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}