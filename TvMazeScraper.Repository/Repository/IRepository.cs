using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TvMazeScraper.Repository.Repository
{
    /// <summary>
    /// interface of repository pattern
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets the record by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> Get(Guid id);

        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAll();

        /// <summary>
        /// Creates record
        /// </summary>
        /// <param name="entities">new entities</param>
        Task Add(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates record
        /// </summary>
        /// <param name="entities">entities to update</param>
        Task Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// Remove all record
        /// </summary>
        Task Remove();

        /// <summary>
        /// Remove record
        /// </summary>
        /// <param name="entities">entities to delete</param>
        Task Remove(IEnumerable<TEntity> entities);
    }
}
