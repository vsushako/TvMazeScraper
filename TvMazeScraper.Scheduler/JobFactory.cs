using System;
using Quartz;
using Quartz.Spi;

namespace TvMazeScraper.Scheduler
{
    public class JobFactory : IJobFactory
    {
        protected readonly IServiceProvider Container;

        public JobFactory(IServiceProvider container)
        {
            Container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var res = Container.GetService(typeof(IJob)) as IJob;
                return res;
            }
            catch (Exception ex)
            {
                //ERROR-  Cannot resolve 'Quartz.Jobs.SendEmailJob' from root provider because it 
                //        requires scoped service 'BLL.Base.UnitOfWork.Interfaces.IUnitOfWork'.
                throw;
            }
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}