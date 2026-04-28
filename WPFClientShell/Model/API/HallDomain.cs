using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;
using WPFClientShell.DataSource;

namespace WPFClientShell.Model.API
{
    public class HallDomain
    {
        private readonly ApiDataSource dataSource;
        
        public HallDomain(ApiDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public async Task<IEnumerable<LobbyDTO>> GetLobbies(int page)
        {
            HttpResponseMessage response = await dataSource.Client.GetAsync($"hall/{page}", HttpCompletionOption.ResponseContentRead);

            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<LobbyDTO>>() ?? Enumerable.Empty<LobbyDTO>();
            }
            else
            {
                return Enumerable.Empty<LobbyDTO>();
            }
        }

        public async Task<LobbyDTO> CreateLobby(CreateLobbyDTO dto)
        {
            HttpResponseMessage response = await dataSource.Client.PostAsJsonAsync("hall/new_lobby", dto);

            response.EnsureSuccessStatusCode();

            LobbyDTO? content = await response.Content.ReadFromJsonAsync<LobbyDTO>();
            return content!;
        }

        public async Task<LobbyDTO> GetLobby(int lobbyId)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            query["id"] = lobbyId.ToString();
            string? pars = query.ToString();

            HttpResponseMessage response = await dataSource.Client.GetAsync($"hall/lobby?{pars}");

            response.EnsureSuccessStatusCode();

            LobbyDTO? content = await response.Content.ReadFromJsonAsync<LobbyDTO>();
            return content!;
        }
    }
}