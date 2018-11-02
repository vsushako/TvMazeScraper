using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TvMazeScraper.Repository;
using TvMazeScraper.Repository.Model;
using TvMazeScraper.Service.Model;
using TvMazeScraper.Service.ViewModel;
using TvMazeScraper.Source;
using TvMazeScraper.Source.Model;

namespace TvMazeScraper.Service
{
    public class ShowsService : IShowsService
    {
        private IUnitOfWorkFactory UnitOfWorkFactory { get; }

        private ITvMazeScraperApi TvMazeScraperApi { get; }

        public ShowsService(IUnitOfWorkFactory unitOfWorkFactory, ITvMazeScraperApi tvMazeScraperApi)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
            TvMazeScraperApi = tvMazeScraperApi;
        }

        public async Task<IEnumerable<ShowOutViewModel>> Get()
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateNew())
                return (await unitOfWork.Show.GetAll()).Select(PareseShowOutViewModel);
        }

        private ShowOutViewModel PareseShowOutViewModel(Show show)
        {
            return new ShowOutViewModel
            {
                Id = show.ExternalId,
                Name = show.Name,
                Cast = show.Cast?.Select(c => new CastViewModel { Name = c.Name, Id = c.ExternalId, Birthday = c.Birthday })
            };
        }

        public async Task Sync()
        {
            var showUpdates = await TvMazeScraperApi.GetUpdates();

            using (var unitOfWork = UnitOfWorkFactory.CreateNew())
            {
                // If come empty array deletes all
                if (showUpdates == null || !showUpdates.Any())
                {
                    await unitOfWork.Show.Remove();
                    unitOfWork.Commit();

                    return;
                }

                var existShows = (await unitOfWork.Show.GetAll())?.ToList();
                
                // If we have no data we need to add all
                if (existShows == null || !existShows.Any())
                {
                    var allShows = await GetShowsFromId(0);
                    await unitOfWork.Show.Add(allShows);
                    unitOfWork.Commit();
                    return;
                }

                var showsForUpdate = new List<Show>();
                var showsForDelete = new List<Show>();

                // Creates array to operate with shows
                foreach (var show in existShows)
                {
                    if (showUpdates.ContainsKey(show.ExternalId.ToString()))
                    {
                        if (showUpdates[show.ExternalId.ToString()] == show.Updated) continue;

                        UpdateShow(show, await TvMazeScraperApi.GetShow(show.ExternalId));
                        showsForUpdate.Add(show);
                    }
                    else
                        showsForDelete.Add(show);
                }

                // Update all shows
                if(showsForUpdate.Any())
                    await unitOfWork.Show.Update(showsForUpdate);

                // Delete all shows
                if (showsForDelete.Any())
                    await unitOfWork.Show.Remove(showsForDelete);

                // Get last id and get all new shows
                var lastId = await unitOfWork.Show.GetLastId();
                await unitOfWork.Show.Add(await GetShowsFromId(lastId.Value + 1));

                unitOfWork.Commit();
            }
        }

        private async Task<IEnumerable<Show>> GetShowsFromId(int id)
        {
            var externalShows = (await TvMazeScraperApi.GetShows((int) Math.Floor((double)id / 250)))?.ToList();
            if (externalShows == null || !externalShows.Any()) return Enumerable.Empty<Show>();

            var shows = externalShows.Where(s => s.id >= id).Select(ParseShow);
            id = externalShows.Max(s => s.id);

            while (externalShows.Any())
            {
                externalShows = (await TvMazeScraperApi.GetShows((int)Math.Floor((double)id / 250)))?.ToList();
                if (externalShows == null || !externalShows.Any()) return Enumerable.Empty<Show>();

                shows = externalShows.Select(ParseShow);
            }

            return shows;
        }

        private Show ParseShow(ShowModel show)
        {
            return new Show { ExternalId = show.id, Name = show.name, Updated = show.updated };
        }

        private void UpdateShow(Show oldShow, ShowModel newShow)
        {
            oldShow.Name = newShow.name;
            oldShow.Updated = newShow.updated;
        }
    }
}
