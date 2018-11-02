using TvMazeScraper.Repository.Repository;

namespace TvMazeScraper.Repository
{
    public class UnitOfWorkFactory<TUnitOfWork> : IUnitOfWorkFactory where TUnitOfWork : IUnitOfWork, new()
    {
        private readonly IMongoDbContext _mongoDbContext;

        private readonly IShowRepository _showRepository;

        public UnitOfWorkFactory(IMongoDbContext mongoDbContext, IShowRepository showRepository)
        {
            _mongoDbContext = mongoDbContext;
            _showRepository = showRepository;
        }

        public IUnitOfWork CreateNew()
        {
            var unitOfWork = new TUnitOfWork { MongoDbContext = _mongoDbContext, Show = _showRepository };
            _showRepository.UnitOfWork = unitOfWork;
            return unitOfWork;
        }
    }
}