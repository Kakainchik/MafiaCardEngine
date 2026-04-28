namespace WPFClientShell.Core
{
    public interface INavigationService
    {
        void Navigate(params object[] parameters);
        Task NavigateAsync(params object[] parameters);
    }
}