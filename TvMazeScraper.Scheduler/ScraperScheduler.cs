using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace TvMazeScraper.Scheduler
{
    public class ScraperScheduler : IScraperScheduler
    {
        private int Interval { get; }
        private IJobFactory JobFactory { get; }

        public ScraperScheduler(int interval, IJobFactory jobFactory)
        {
            Interval = interval;
            JobFactory = jobFactory;
        }

        public async void Start()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            
            scheduler.JobFactory = JobFactory;
            await scheduler.Start();

            var job = JobBuilder.Create<TvMazeScraper>().Build();

            // Create trigget
            var trigger = TriggerBuilder.Create() 
                .WithIdentity("trigger", "TvMaze")     
                .StartNow()                            
                .WithSimpleSchedule(x => x            
                    .WithIntervalInMinutes(Interval)          
                    .RepeatForever())                   
                .Build();

            // Schedule job
            await scheduler.ScheduleJob(job, trigger);        
        }
    }
}