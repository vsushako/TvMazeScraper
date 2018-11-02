using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TvMazeScraper.Repository;
using TvMazeScraper.Service;

namespace TvMazeScraper.IoC
{
    public static class InjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IShowsService, ShowsService>();
            services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory<UnitOfWork>>();
            services.AddScoped<IMongoDbContext>(s => new MongoDbContext("MyConnectionString"));
            
        }
    }
}
