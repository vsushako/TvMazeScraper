using MongoDB.Driver;
using TvMazeScraper.Repository.Repository;

namespace TvMazeScraper.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private IClientSession _session;

        public IMongoDbContext MongoDbContext { get; set; }

        public IShowRepository Show { get; set; }

        public IClientSession Session
        {
            get
            {
                if (_session != null) return _session;

                _session = MongoDbContext.TvMazeShows.Client.StartSession();
                _session.StartTransaction();

                return _session;
            }
        }

        public void Dispose()
        {
            Session.AbortTransaction();
        }

        public void Commit()
        {
            Session.CommitTransaction();
            // Removes session
            _session = null;
        }
    }

    public interface IMongoDbContext
    {
        IMongoDatabase TvMazeShows { get; }
    }

    public class MongoDbContext: IMongoDbContext
    {
        internal IMongoClient Client { get; }

        private IMongoDatabase _show;
        
        public MongoDbContext(string connectionString) => Client = new MongoClient(connectionString);
        
        public IMongoDatabase TvMazeShows => _show ?? (_show = Client.GetDatabase(nameof(TvMazeShows)));
    }
}