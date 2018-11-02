using System.Collections.Generic;
using System.Threading.Tasks;
using TvMazeScraper.Service.Model;
using TvMazeScraper.Service.ViewModel;

namespace TvMazeScraper.Service
{
    public interface IShowsService
    {
        Task<IEnumerable<ShowOutViewModel>> Get();

        Task Sync();
    }
}