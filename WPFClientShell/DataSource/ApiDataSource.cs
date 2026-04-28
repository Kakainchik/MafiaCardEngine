using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace WPFClientShell.DataSource
{
    public class ApiDataSource : IDisposable
    {
        private readonly HttpClientHandler handler;
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookies;

        private bool disposedValue;

        public HttpClient Client => httpClient;
        public CookieContainer Cookies => cookies;

        public ApiDataSource(string apiUrl)
        {
            cookies = new CookieContainer();

            handler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = cookies
            };
            httpClient = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(apiUrl)
            };

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("MafiaCardWPF",
                    Assembly.GetEntryAssembly()?.GetName().Version?.ToString()));
        }

        public void UpdateBearerHeader(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // Dispose managed state (managed objects)
                    httpClient.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
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