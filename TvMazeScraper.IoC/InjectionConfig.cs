using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using TvMazeScraper.Repository;
using TvMazeScraper.Repository.Repository;
using TvMazeScraper.Scheduler;
using TvMazeScraper.Service;
using TvMazeScraper.Source;

namespace TvMazeScraper.IoC
{
    public static class InjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IShowsService, ShowsService>();
            services.AddTransient<IRequestSender, HttpRequestSender>();

            services.AddTransient<ITvMazeScraperApi>(provider => new TvMazeScraperApi(provider.GetService<IRequestSender>(), configuration["tvMaze:address"]));
            
            services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory<UnitOfWork>>();
            services.AddTransient<IShowRepository, ShowRepository>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<IJob, Scheduler.TvMazeScraper>();
            services.AddTransient<IMongoDbContext>(provider => new MongoDbContext(configuration["database:connection"]));
            var schedulerInterval = configuration["scheduler:interval"];

            // if interval not set get updates each hour
            if (!int.TryParse(schedulerInterval, out var interval))
                interval = 60;

            services.AddTransient<IScraperScheduler>(provider => new ScraperScheduler(interval, provider.GetService<IJobFactory>()));
        }
    }
}
