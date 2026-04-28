namespace WPFClientShell.Core
{
    public class ViewModelRouter
    {
        private readonly IDictionary<string, INavigationService> routes;

        public ViewModelRouter(IDictionary<string, INavigationService> routes)
        {
            this.routes = routes;
        }

        public void NavigateTo(string url, params object[] parameters)
        {
            if(routes.ContainsKey(url))
            {
                routes[url].Navigate(parameters);
            }
        }

        public async Task NavigateToAsync(string url, params object[] parameters)
        {
            if(routes.ContainsKey(url))
            {
                await routes[url].NavigateAsync(parameters);
            }
        }
    }
}