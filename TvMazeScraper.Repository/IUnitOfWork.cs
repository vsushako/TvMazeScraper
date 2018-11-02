using System;
using MongoDB.Driver;
using TvMazeScraper.Repository.Repository;

namespace TvMazeScraper.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IClientSession Session { get; }

        IMongoDbContext MongoDbContext { get; set; }

        IShowRepository Show { get; set; }

        void Commit();
    }
}
