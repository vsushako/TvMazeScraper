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

            if (!int.TryParse(configuration["callManager:waitPeriod"], out var waitPeriod))
                waitPeriod = 1000;

            if (!int.TryParse(configuration["callManager:elementsCount"], out var elementsCount))
                elementsCount = 20;

            services.AddTransient<ICallManager>(provider => new CallManager(waitPeriod) { ElementsCount = elementsCount });
            services.AddHttpClient<IRequestSender, TvMazeClient>(client =>
                client.BaseAddress = new Uri(configuration["tvMaze:address"]));

            services.AddTransient<ITvMazeScraperApi, TvMazeScraperApi>();

            services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory<UnitOfWork>>();
            services.AddTransient<IShowRepository, ShowRepository>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<IJob, Scheduler.TvMazeScraper>();
            services.AddTransient<IMongoDbContext>(provider => new MongoDbContext(configuration["database:connection"]));

            // if interval not set get updates each hour
            if (!int.TryParse(configuration["scheduler:interval"], out var interval))
                interval = 60;

            services.AddTransient<IScraperScheduler>(provider => new ScraperScheduler(interval, provider.GetService<IJobFactory>()));
        }
    }
}
