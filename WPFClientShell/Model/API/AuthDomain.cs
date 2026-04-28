using Microsoft.Extensions.Options;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;
using WPFClientShell.DataSource;

namespace WPFClientShell.Model.API
{
    public class AuthDomain
    {
        private const string RT_COOKIE_NAME = "RefreshToken";

        private readonly IOptionsMonitor<AuthSettings> authDelegate;
        private readonly ApiDataSource dataSource;

        public bool IsAuthenticated => !string.IsNullOrEmpty(authDelegate.CurrentValue.JWT);

        public string? Username => authDelegate.CurrentValue.Username;

        public AuthDomain(IOptionsMonitor<AuthSettings> authDelegate, ApiDataSource dataSource)
        {
            this.authDelegate = authDelegate;
            this.dataSource = dataSource;

            if(IsAuthenticated)
            {
                dataSource.UpdateBearerHeader(authDelegate.CurrentValue.JWT!);
            }
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            AuthRequest body = new AuthRequest(username, password);
            HttpResponseMessage response = await dataSource.Client.PostAsJsonAsync("auth/sign_in", body);

            if(response.IsSuccessStatusCode)
            {
                await SubmitAuthResponse(response);
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Register(string username, string password)
        {
            AuthRequest body = new AuthRequest(username, password);
            HttpResponseMessage response = await dataSource.Client.PostAsJsonAsync("auth/sign_up", body);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RefreshToken(string? refreshToken = null)
        {
            HttpResponseMessage response;
            if(string.IsNullOrEmpty(refreshToken))
            {
                //Refresh token is in headers
                response = await dataSource.Client.PostAsync("auth/refresh_token", null);
            }
            else
            {
                NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
                query[RT_COOKIE_NAME] = refreshToken;

                response = await dataSource.Client.PostAsync($"auth/refresh_token?{query.ToString()}", null);
            }

            if(response.IsSuccessStatusCode)
            {
                await SubmitAuthResponse(response);
            }

            return response.IsSuccessStatusCode;
        }

        private async Task SubmitAuthResponse(HttpResponseMessage response)
        {
            AuthResponse? content = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if(content is null)
            {
                throw new HttpIOException(HttpRequestError.InvalidResponse, $"Cannot obtain {nameof(AuthResponse)} from the content.");
            }

            string? rtoken = dataSource.Cookies.GetAllCookies()["RefreshToken"]?.Value;

            authDelegate.CurrentValue.UserId = content.Id;
            authDelegate.CurrentValue.Username = content.Username;
            authDelegate.CurrentValue.JWT = content.JwtToken;
            authDelegate.CurrentValue.RefreshToken = rtoken;

            dataSource.UpdateBearerHeader(content.JwtToken);
        }
    }
}