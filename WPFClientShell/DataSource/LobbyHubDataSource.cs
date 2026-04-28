using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Authentication;

namespace WPFClientShell.DataSource
{
    public class LobbyHubDataSource : IDisposable
    {
        private readonly TimeSpan[] reattemptions = new[]
        {
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30)
        };

        private readonly HubConnection hubConnection;

        private bool disposedValue;
        private IList<IDisposable> listeners;
        private string? accessToken;

        public LobbyHubDataSource(string hubUrl, string? accessToken)
        {
            listeners = new List<IDisposable>();

            Uri uri = new Uri(hubUrl);
            hubConnection = new HubConnectionBuilder()
                .WithUrl(uri.AbsoluteUri + "lobby", options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(accessToken);
                    };
                })
                .WithAutomaticReconnect(reattemptions)
                .Build();

            this.accessToken = accessToken;
        }

        public void EnsureHasAccessToken()
        {
            if(accessToken is null)
            {
                throw new AuthenticationException("The access token is not present.");
            }
        }

        public async Task ConnectAsync()
        {
            await hubConnection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await hubConnection.StopAsync();
        }

        public async Task<T> SendWithResultAsync<T>(string methodName, object? obj1)
        {
            return await hubConnection.InvokeAsync<T>(methodName, obj1);
        }

        public async Task SendAsync(string methodName, object? obj1)
        {
            await hubConnection.SendAsync(methodName, obj1);
        }

        public void ListenToMethod<T>(string methodName, Action<T> contextHandler)
        {
            IDisposable listener = hubConnection.On<T>(methodName, contextHandler);
            listeners.Add(listener);
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // Dispose managed state (managed objects)
                    hubConnection.DisposeAsync();

                    foreach(IDisposable listener in listeners)
                    {
                        listener.Dispose();
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}