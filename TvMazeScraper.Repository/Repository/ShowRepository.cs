using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using TvMazeScraper.Repository.Model;

namespace TvMazeScraper.Repository.Repository
{
    public class ShowRepository: IShowRepository
    {
        public IUnitOfWork UnitOfWork { get; set; }

        private IMongoDbContext _mongoDbContext;

        // Get context from unitOfWork or from di
        private IMongoDbContext MongoDbContext => _mongoDbContext ?? (_mongoDbContext = UnitOfWork.MongoDbContext);

        public ShowRepository() { }

        public ShowRepository(IMongoDbContext mongoDbContext) => _mongoDbContext = mongoDbContext;

        public async Task<Show> Get(Guid id)
        {
            return await MongoDbContext
                .TvMazeShows
                .GetCollection<Show>(nameof(Show))
                .Find(s => s.Id == id)
                .SingleAsync();
        }

        public async Task<IEnumerable<Show>> GetAll()
        {
            return await MongoDbContext
                .TvMazeShows
                .GetCollection<Show>(nameof(Show))
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task Add(IEnumerable<Show> entities)
        {
            await MongoDbContext
                .TvMazeShows
                .GetCollection<Show>(nameof(Show))
                .InsertManyAsync((IClientSessionHandle) UnitOfWork.Session, entities);
        }

        public async Task Update(IEnumerable<Show> entities)
        {
            var filter = new FilterDefinitionBuilder<Show>()
                .In(m => m.Id, entities.Select(e => e.Id));

            foreach (var entity in entities)
                await MongoDbContext
                    .TvMazeShows
                    .GetCollection<Show>(nameof(Show))
                    .ReplaceOneAsync((IClientSessionHandle) UnitOfWork.Session, filter, entity);
        }

        public async Task Remove()
        {
            await MongoDbContext
                .TvMazeShows
                .GetCollection<Show>(nameof(Show))
                .DeleteManyAsync(_ => true);
        }

        public async Task Remove(IEnumerable<Show> entities)
        {
            var filter = new FilterDefinitionBuilder<Show>()
                .In(m => m.Id, entities.Select(e => e.Id));

            await MongoDbContext
                .TvMazeShows
                .GetCollection<Show>(nameof(Show))
                .DeleteManyAsync(filter);
        }

        public async Task<int?> GetLastId()
        {
            return (await MongoDbContext
                .TvMazeShows
                .GetCollection<Show>(nameof(Show))
                .Find(_ => true)
                .SortByDescending(d => d.ExternalId)
                .SingleOrDefaultAsync())
                ?.Updated;
        }
    }
}
