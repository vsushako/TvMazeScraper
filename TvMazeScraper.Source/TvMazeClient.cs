using System.Net.Http;
using System.Threading.Tasks;

namespace TvMazeScraper.Source
{
    public class TvMazeClient : IRequestSender
    {
        private readonly HttpClient _client;

        public TvMazeClient(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            _client = httpClient;
        }

        public async Task<string> Get(string url)
        {
            return await _client.GetStringAsync(url);
        }
    }
}
