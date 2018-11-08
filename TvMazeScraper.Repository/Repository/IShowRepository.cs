using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TvMazeScraper.Repository.Model;

namespace TvMazeScraper.Repository.Repository
{
    public interface IShowRepository: IRepository<Show>
    {
        IUnitOfWork UnitOfWork { get; set; }
        
        /// <summary>
        /// Gets last inserted id
        /// </summary>
        /// <returns></returns>
        Task<int?> GetLastId();

        /// <summary>
        /// Gets some elements array
        /// </summary>
        /// <param name="from">element</param>
        /// <param name="to">to element</param>
        /// <returns></returns>
        Task<IEnumerable<Show>> Get(int from, int to);
    }
}
