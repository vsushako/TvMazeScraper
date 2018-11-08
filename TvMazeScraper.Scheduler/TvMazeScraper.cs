using System.Threading.Tasks;
using Quartz;
using TvMazeScraper.Service;

namespace TvMazeScraper.Scheduler
{
    public class TvMazeScraper : IJob
    {
        private IShowsService ShowsService { get; }

        public TvMazeScraper(IShowsService showsService) => ShowsService = showsService;

        public async Task Execute(IJobExecutionContext context) => await ShowsService.Sync();
    }
}
