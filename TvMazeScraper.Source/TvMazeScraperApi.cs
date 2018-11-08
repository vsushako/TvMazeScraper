using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TvMazeScraper.Source.Model;
using System.Configuration;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;

namespace TvMazeScraper.Source
{
    public class TvMazeScraperApi : ITvMazeScraperApi
    {
        private IRequestSender RequestSender { get; }

        private int _theLock = 0;

        public TvMazeScraperApi(IRequestSender requestSender, string address)
        {
            RequestSender = requestSender;
            RequestSender.Address = address;
        }

        public async Task<IEnumerable<CastModel>> GetCast(int showId)
        {
            return await GetResult<IEnumerable<CastModel>>($"/shows/{showId}/cast");
        }

        public async Task<ShowModel> GetShow(int id)
        {
            return await GetResult<ShowModel>($"/shows/{id}");
        }

        public async Task<IEnumerable<ShowModel>> GetShows(int page = 0)
        {
            return await GetResult<IEnumerable<ShowModel>>($"/shows?page={page}");
        }

        public async Task<IDictionary<string, int>> GetUpdates()
        {
            return await GetResult<IDictionary<string, int>>("/updates/shows");
        }

        private async Task<TResult> GetResult<TResult>(string address)
        {
            var resultStr = "";

            try
            {
                // We come here only from scheduler, so no need lock enother threads
                while (Interlocked.CompareExchange(ref _theLock, 0, 0) == 1)
                {
                    // wait if we have reached the timeout
                    Thread.Sleep(5 * 1000);
                }

                resultStr = await RequestSender.Get(address);
            }
            catch (Exception e)
            {
                // If we reache timeout when wait
                if (((e.InnerException as WebException)?.Response as HttpWebResponse)?.StatusCode !=
                    HttpStatusCode.TooManyRequests) return default(TResult); 

                Interlocked.Increment(ref _theLock);
                Thread.Sleep(5 * 1000);
                Interlocked.Decrement(ref _theLock);

                return await GetResult<TResult>(address);
            }

            return JsonConvert.DeserializeObject<TResult>(resultStr);
        }
    }
}
