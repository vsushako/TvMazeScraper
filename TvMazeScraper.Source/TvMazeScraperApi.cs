using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TvMazeScraper.Source.Model;
using System.Configuration;
using System.Runtime.Serialization;

namespace TvMazeScraper.Source
{
    public class TvMazeScraperApi : ITvMazeScraperApi
    {
        private IRequestSender RequestSender { get; }

        public TvMazeScraperApi(IRequestSender requestSender)
        {
            RequestSender = requestSender;
        }

        public async Task<IEnumerable<CastModel>> GetCast(int showId)
        {
            var resultStr = await RequestSender.Get($"/shows/{showId}/cast");
            
            return JsonConvert.DeserializeObject<IEnumerable<CastModel>>(resultStr);
        }

        public async Task<ShowModel> GetShow(int id)
        {
            var resultStr = await RequestSender.Get($"/shows/{id}");

            return JsonConvert.DeserializeObject<ShowModel>(resultStr);
        }

        public async Task<IEnumerable<ShowModel>> GetShows(int page = 0)
        {
            var resultStr = await RequestSender.Get($"/shows?page={page}");

            return JsonConvert.DeserializeObject<IEnumerable<ShowModel>>(resultStr);
        }

        public async Task<IDictionary<string, int>> GetUpdates()
        {
            var resultStr = await RequestSender.Get("/updates/shows");

            return JsonConvert.DeserializeObject<IDictionary<string, int>>(resultStr);
        }
    }
}
