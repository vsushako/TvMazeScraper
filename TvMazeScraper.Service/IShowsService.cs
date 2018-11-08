using System.Collections.Generic;
using System.Threading.Tasks;
using TvMazeScraper.Service.Model;
using TvMazeScraper.Service.ViewModel;

namespace TvMazeScraper.Service
{
    public interface IShowsService
    {
        /// <summary>
        /// Gets all elements
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ShowOutViewModel>> Get();

        /// <summary>
        /// Gets elements by pages
        /// </summary>
        /// <param name="page">page number</param>
        /// <returns></returns>
        Task<IEnumerable<ShowOutViewModel>> Get(int page);

        /// <summary>
        /// Syncronize database
        /// </summary>
        /// <returns></returns>
        Task Sync();
    }
}