using System.Collections.Generic;
using System.Threading.Tasks;
using TvMazeScraper.Source.Model;

namespace TvMazeScraper.Source
{
    /// <summary>
    /// Service to communicate with TvMaze
    /// </summary>
    public interface ITvMazeScraperApi
    {
        /// <summary>
        /// Get all cast in show
        /// </summary>
        /// <param name="showId">show id</param>
        /// <returns></returns>
        Task<IEnumerable<CastModel>> GetCast(int showId);

        /// <summary>
        /// Get show by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ShowModel> GetShow(int id);

        /// <summary>
        /// Get all shows
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ShowModel>> GetShows(int page);

        /// <summary>
        /// Get all shows updates
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, int>> GetUpdates();
    }
}