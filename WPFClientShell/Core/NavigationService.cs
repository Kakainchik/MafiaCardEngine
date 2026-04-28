namespace WPFClientShell.Core
{
    public class NavigationService<TViewModel> : INavigationService where TViewModel : BaseViewModel
    {
        private readonly NavigationStore navigationStore;
        private readonly Func<TViewModel> createViewModel;

        public NavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            this.navigationStore = navigationStore;
            this.createViewModel = createViewModel;
        }

        public void Navigate(params object[] parameters)
        {
            TViewModel viewModel = createViewModel();
            if(parameters.Length > 0 && viewModel is IParameterNavigationService)
            {
                ((IParameterNavigationService)viewModel).ParameterInitialize(parameters);
            }

            navigationStore.CurrentViewModel = viewModel;
        }

        public async Task NavigateAsync(params object[] parameters)
        {
            TViewModel viewModel = createViewModel();
            if(parameters.Length > 0 && viewModel is IParameterNavigationService)
            {
                ((IParameterNavigationService)viewModel).ParameterInitialize(parameters);
            }

            if(viewModel is IAsyncNavigationService)
            {
                await ((IAsyncNavigationService)viewModel).InitializeAsync();
            }

            navigationStore.CurrentViewModel = viewModel;
        }
    }
}