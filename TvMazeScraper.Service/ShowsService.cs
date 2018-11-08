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

        public async Task<IEnumerable<ShowOutViewModel>> Get(int page)
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateNew())
                return (await unitOfWork.Show.Get(page * 250, ++page * 250)).Select(PareseShowOutViewModel);
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
                var newShows = (await GetShowsFromId(lastId.Value + 1)).ToList();
                if(newShows.Any())
                    await unitOfWork.Show.Add(newShows);

                unitOfWork.Commit();
            }
        }

        private async Task<IEnumerable<Show>> GetShowsFromId(int id)
        {
            var page = (int)Math.Floor((double)id / 250);
            var externalShows = (await TvMazeScraperApi.GetShows(page))?.ToList();
            if (externalShows == null || !externalShows.Any()) return Enumerable.Empty<Show>();

            var shows = (await ParseShows(externalShows.Where(s => s.id >= id)))?.ToList();
            while (externalShows.Any())
            {
                externalShows = (await TvMazeScraperApi.GetShows(++page))?.ToList();
                if (externalShows == null || !externalShows.Any()) return shows;

                shows?.AddRange(await ParseShows(externalShows));
            }

            return shows;
        }

        private async Task<IEnumerable<Show>> ParseShows(IEnumerable<ShowModel> shows)
        {
            var result = new List<Show>();
            foreach (var t in shows)
            {
                var cast = await TvMazeScraperApi.GetCast(t.id);
                result.Add(new Show { ExternalId = t.id, Name = t.name, Updated = t.updated, Cast = ParseCast(cast) });
            }
            
            return result;
        }

        private IEnumerable<Cast> ParseCast(IEnumerable<CastModel> cast)
        {
            var result = new List<Cast>();
            foreach (var c in cast)
            {
                DateTime.TryParse(c.person.birthday, out var birthday);
                result.Add(new Cast { Birthday = birthday, ExternalId = c.person.id, Name = c.person.name });
            }

            return result;
        }

        private void UpdateShow(Show oldShow, ShowModel newShow)
        {
            if (newShow == null) return;

            oldShow.Name = newShow.name;
            oldShow.Updated = newShow.updated;
        }
    }
}
