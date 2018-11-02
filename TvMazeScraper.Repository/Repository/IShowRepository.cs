using System;
using System.Threading.Tasks;
using TvMazeScraper.Repository.Model;

namespace TvMazeScraper.Repository.Repository
{
    public interface IShowRepository: IRepository<Show>
    {
        IUnitOfWork UnitOfWork { get; set; }
        
        Task<int?> GetLastId();
    }
}
