using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TvMazeScraper.Repository;
using TvMazeScraper.Repository.Model;
using TvMazeScraper.Repository.Repository;
using TvMazeScraper.Service;
using TvMazeScraper.Source;
using TvMazeScraper.Source.Model;

namespace TvMazeScraper.ServiceTest
{
    [TestClass]
    public class ServiceTest
    {
        private Mock<IShowRepository> _showRepository;

        private Mock<ITvMazeScraperApi> _tvMazeScraperApi;

        private IUnitOfWorkFactory SetupGetShowRepository(IEnumerable<Show> shows)
        {
            _showRepository = new Mock<IShowRepository> { CallBase = false };
            _showRepository.Setup(s => s.GetAll()).ReturnsAsync(shows);

            var mockUnitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            mockUnitOfWork.Setup(u => u.Show).Returns(_showRepository.Object);

            var mockFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            mockFactory.Setup(m => m.CreateNew()).Returns(mockUnitOfWork.Object);

            return mockFactory.Object;
        }

        private IUnitOfWorkFactory SetupUpsertShowRepository(IEnumerable<Show> shows)
        {
            _showRepository = new Mock<IShowRepository> { CallBase = false };
            _showRepository.Setup(s => s.GetAll()).ReturnsAsync(shows);
            _showRepository.Setup(s => s.GetLastId()).ReturnsAsync(shows?.Max(s => s.ExternalId));

            var mockUnitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            mockUnitOfWork.Setup(u => u.Show).Returns(_showRepository.Object);

            var mockFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            mockFactory.Setup(m => m.CreateNew()).Returns(mockUnitOfWork.Object);

            return mockFactory.Object;
        }

        private ITvMazeScraperApi SetupTvMazeServer(IDictionary<string, int> updated, ShowModel[] showInViewModels = null)
        {
            _tvMazeScraperApi = new Mock<ITvMazeScraperApi> { CallBase = false };
            _tvMazeScraperApi.Setup(t => t.GetUpdates()).ReturnsAsync(updated);
            _tvMazeScraperApi.Setup(t => t.GetShows(It.IsAny<int>())).ReturnsAsync((int a) =>
            {
                return a * 250 >= showInViewModels?.Max(s => s.id) ? showInViewModels : null;
            });
            
            return _tvMazeScraperApi.Object;
        }

        [TestMethod]
        public async Task ServiceTest_CheckGetEmpty_ExpectOk()
        {
            var data = Enumerable.Empty<Show>();

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task ServiceTest_CheckGet_ExpectOk()
        {
            var data = new [] { new Show { ExternalId = 444,Name = "Test"} };

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(data[0].ExternalId, result.First().Id);
            Assert.AreEqual(data[0].Name, result.First().Name);
        }

        [TestMethod]
        public async Task ServiceTest_CheckGetMultiple_ExpectOk()
        {
            var data = new[] { new Show { ExternalId = 1, Name = "Test1" }, new Show { ExternalId = 2, Name = "Test2" } };

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(data[0].ExternalId, result.First().Id);
            Assert.AreEqual(data[0].Name, result.First().Name);

            Assert.AreEqual(data[1].ExternalId, result.Last().Id);
            Assert.AreEqual(data[1].Name, result.Last().Name);
        }

        [TestMethod]
        public async Task ServiceTest_CheckCastIsEmpty_ExpectOk()
        {
            var data = new[] { new Show { ExternalId = 444, Name = "Test" } };

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(null, result.First().Cast);
        }

        [TestMethod]
        public async Task ServiceTest_CheckCastOne_ExpectOk()
        {
            var data = new[] { new Show { ExternalId = 444, Name = "Test", Cast = new [] { new Cast { Name = "test", ExternalId = 1, Birthday = new DateTime() } } } };

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(data[0].Cast.First().Name, result.First().Cast.First().Name);
            Assert.AreEqual(data[0].Cast.First().ExternalId, result.First().Cast.First().Id);
            Assert.AreEqual(data[0].Cast.First().Birthday, result.First().Cast.First().Birthday);
        }

        [TestMethod]
        public async Task ServiceTest_CheckCastOneOfTwo_ExpectOk()
        {
            var data = new[] { new Show { ExternalId = 111, Name = "TestEmpty" }, new Show { ExternalId = 444, Name = "Test", Cast = new[] { new Cast { Name = "test", ExternalId = 1, Birthday = new DateTime() } } } };

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(1, result.Count(d => d.Cast != null));
            Assert.AreEqual(1, result.Count(d => d.Cast == null));
            
            Assert.AreEqual(data.First(d => d.Cast != null).Cast.First().Name, result.First(d => d.Cast != null).Cast.First().Name);
            Assert.AreEqual(data.First(d => d.Cast != null).Cast.First().ExternalId, result.First(d => d.Cast != null).Cast.First().Id);
            Assert.AreEqual(data.First(d => d.Cast != null).Cast.First().Birthday, result.First(d => d.Cast != null).Cast.First().Birthday);
        }

        [TestMethod]
        public async Task ServiceTest_CheckCastMultiple_ExpectOk()
        {
            var data = new[] { new Show { ExternalId = 444, Name = "Test", Cast = new[] { new Cast { Name = "test", ExternalId = 1, Birthday = new DateTime() }, new Cast { Name = "test2", ExternalId = 2, Birthday = new DateTime() } } } };

            var service = new ShowsService(SetupGetShowRepository(data), null);
            var result = await service.Get();

            Assert.AreEqual(2, result.First(d => d.Cast != null).Cast.Count());
            Assert.AreEqual(1, result.Count(d => d.Cast != null));
            Assert.AreEqual(0, result.Count(d => d.Cast == null));

            var firstCast = result.First().Cast.First(c => c.Id == data.First().Cast.First().ExternalId);
            Assert.AreEqual(data.First().Cast.First().Name, firstCast.Name);
            Assert.AreEqual(data.First().Cast.First().Birthday, firstCast.Birthday);

            var lastCast = result.First().Cast.First(c => c.Id == data.First().Cast.Last().ExternalId);

            Assert.AreEqual(data.First().Cast.Last().Name, lastCast.Name);
            Assert.AreEqual(data.First().Cast.Last().Birthday, lastCast.Birthday);
        }

        [TestMethod]
        public async Task ServiceTest_CheckUpsertLastUpdatedExist_ExpectOk()
        {
            var service = new ShowsService(SetupUpsertShowRepository(new [] { new Show { Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } }), SetupTvMazeServer(new Dictionary<string, int> { { "1", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } }));

            await service.Sync();

            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }
        
        [TestMethod]
        public async Task ServiceTest_CheckUpsertLastUpdatedExistDeleteOthers_ExpectOk()
        {
            var existShows = new[]
            {
                new Show {ExternalId = 1, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new Show {ExternalId = 2, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new Show {ExternalId = 4, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            var service = new ShowsService(SetupUpsertShowRepository(existShows), SetupTvMazeServer(new Dictionary<string, int> { { "1", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } }));

            await service.Sync();
            _showRepository.Setup(s => s.Remove(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(elements =>
            {
                Assert.AreEqual(existShows.Length, elements.Count());
                foreach (var s in existShows.Where(s => s.ExternalId != 1))
                {
                    Assert.AreEqual(1, elements.Count(u => u.Id == s.Id));
                }
            }).Returns(Task.FromResult(true));

            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Once);
        }

        [TestMethod]
        public async Task ServiceTest_CheckUpsertEmpty_ExpectOk()
        {
            var service = new ShowsService(SetupUpsertShowRepository(null), SetupTvMazeServer(new Dictionary<string, int> {}));

            await service.Sync();

            _showRepository.Verify(s => s.GetAll(), Times.Never);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }

        [TestMethod]
        public async Task ServiceTest_CheckUpsertNull_ExpectOk()
        {
            var service = new ShowsService(SetupUpsertShowRepository(null), SetupTvMazeServer(null));

            await service.Sync();

            _showRepository.Verify(s => s.GetAll(), Times.Never);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }

        [TestMethod]
        public async Task ServiceTest_CheckUpdateOne_ExpectOk()
        {
            var newShow = new ShowModel
            {
                id = 1,
                name = "test",
                updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var factory = SetupUpsertShowRepository(new[] {new Show {ExternalId = 1, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } });

            _showRepository.Setup(s => s.Update(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(u =>
            {
                Assert.AreEqual(1, u.Count());
                Assert.AreEqual(newShow.name, u.First().Name);
                Assert.AreEqual(newShow.id, u.First().ExternalId);
                Assert.AreEqual(newShow.updated, u.First().Updated);
            }).Returns(Task.FromResult(true));

            var service = new ShowsService(factory, SetupTvMazeServer(new Dictionary<string, int> { { "1", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } }, new [] { newShow }));

            await service.Sync();
            _showRepository.Verify(s => s.GetAll(), Times.Once);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Once);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }

        [TestMethod]
        public async Task ServiceTest_CheckUpdateMany_ExpectOk()
        {
            var newShows = new[] {
                new ShowModel { id = 1, name = "test",  updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 2, name = "test2", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 3, name = "test3", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 4, name = "test4", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            };
            var newShowsUpdates = new Dictionary<string, int>
            {
                { "1", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "2", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "3", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "4", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            var factory = SetupUpsertShowRepository(
                new[] { new Show { ExternalId = 1, Name = "OldName", Updated = (int)DateTimeOffset.Now.ToUnixTimeSeconds() },
                        new Show { ExternalId = 2, Name = "OldName", Updated = (int)DateTimeOffset.Now.ToUnixTimeSeconds() },
                        new Show { ExternalId = 3, Name = "OldName", Updated = (int)DateTimeOffset.Now.ToUnixTimeSeconds() },
                        new Show { ExternalId = 4, Name = "OldName", Updated = (int)DateTimeOffset.Now.ToUnixTimeSeconds() } });

            _showRepository.Setup(s => s.Update(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(elements =>
            {
                Assert.AreEqual(newShows.Length, elements.Count());
                foreach (var s in newShows)
                {
                    Assert.AreEqual(s.name, elements.First(u => u.ExternalId == s.id).Name);
                    Assert.AreEqual(new DateTime(s.updated), elements.First(u => u.ExternalId == s.id).Updated);
                }
            }).Returns(Task.FromResult(true));
            var service = new ShowsService(factory, SetupTvMazeServer(newShowsUpdates, newShows));

            await service.Sync();
            _showRepository.Verify(s => s.GetAll(), Times.Once);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Once);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }

        [TestMethod]
        public async Task ServiceTest_CheckAddOne_ExpectOk()
        {
            var date = DateTime.Now;
            var newShow = new ShowModel
            {
                id = 1,
                name = "test",
                updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var factory = SetupUpsertShowRepository(null);

            _showRepository.Setup(s => s.Add(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(u =>
            {
                Assert.AreEqual(1, u.Count());
                Assert.AreEqual(newShow.name, u.First().Name);
                Assert.AreEqual(newShow.id, u.First().ExternalId);
                Assert.AreEqual(new DateTime(newShow.updated), u.First().Updated);
            }).Returns(Task.FromResult(true));

            var service = new ShowsService(factory, SetupTvMazeServer(new Dictionary<string, int> { { "1", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } }, new[] { newShow }));

            await service.Sync();
            _showRepository.Verify(s => s.GetAll(), Times.Once);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Once);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }

        [TestMethod]
        public async Task ServiceTest_CheckAddMany_ExpectOk()
        {
            var newShows = new[] {
                new ShowModel { id = 1, name = "test",  updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 2, name = "test2", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 3, name = "test3", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 4, name = "test4", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            };

            var newShowsUpdates = new Dictionary<string, int>
            {
                { "1", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "2", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "3", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "4", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            var factory = SetupUpsertShowRepository(null);

            _showRepository.Setup(s => s.Update(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(elements =>
            {
                Assert.AreEqual(newShows.Length, elements.Count());
                foreach (var s in newShows)
                {
                    Assert.AreEqual(s.name, elements.First(u => u.ExternalId == s.id).Name);
                    Assert.AreEqual(new DateTime(s.updated), elements.First(u => u.ExternalId == s.id).Updated);
                }
            }).Returns(Task.FromResult(true));
            var service = new ShowsService(factory, SetupTvMazeServer(newShowsUpdates, newShows));

            await service.Sync();
            _showRepository.Verify(s => s.GetAll(), Times.Once);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Once);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Never);
        }

        [TestMethod]
        public async Task ServiceTest_CheckDeleteOne_ExpectOk()
        {
            var newShow = new ShowModel
            {
                id = 2,
                name = "test",
                updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var existShow = new Show { ExternalId = 1, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() };

            var factory = SetupUpsertShowRepository(new[] { existShow });

            _showRepository.Setup(s => s.Add(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(u =>
            {
                Assert.AreEqual(1, u.Count());
                Assert.AreEqual(newShow.name, u.First().Name);
                Assert.AreEqual(newShow.id, u.First().ExternalId);
                Assert.AreEqual(new DateTime(newShow.updated), u.First().Updated);
            }).Returns(Task.FromResult(true));

            _showRepository.Setup(s => s.Remove(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(u =>
            {
                Assert.AreEqual(1, u.Count());
                Assert.AreEqual(existShow.Id, u.First().Id);
            }).Returns(Task.FromResult(true));

            var service = new ShowsService(factory, SetupTvMazeServer(new Dictionary<string, int> { { "2", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() } }, new [] { newShow }));

            await service.Sync();
            _showRepository.Verify(s => s.GetAll(), Times.Once);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Once);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Once);
        }

        [TestMethod]
        public async Task ServiceTest_CheckDeleteMany_ExpectOk()
        {
            var newShows = new[] {
                new ShowModel { id = 5, name = "test",  updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 6, name = "test2", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 7, name = "test3", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new ShowModel { id = 8, name = "test4", updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            };
            var newShowsUpdates = new Dictionary<string, int>
            {
                { "5", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "6", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "7", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "8", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
            var existShows = new[]
            {
                new Show { ExternalId = 1, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new Show { ExternalId = 2, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new Show { ExternalId = 4, Name = "OldName", Updated = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
            var factory = SetupUpsertShowRepository(existShows);

            _showRepository.Setup(s => s.Add(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(elements =>
            {
                Assert.AreEqual(newShows.Length, elements.Count());
                foreach (var s in newShows)
                {
                    Assert.AreEqual(s.name, elements.First(u => u.ExternalId == s.id).Name);
                    Assert.AreEqual(new DateTime(s.updated), elements.First(u => u.ExternalId == s.id).Updated);
                }
            }).Returns(Task.FromResult(true));
            _showRepository.Setup(s => s.Remove(It.IsAny<IEnumerable<Show>>())).Callback<IEnumerable<Show>>(elements =>
            {
                Assert.AreEqual(existShows.Length, elements.Count());
                foreach (var s in existShows)
                {
                    Assert.AreEqual(1, elements.Count(u => u.Id == s.Id));
                }
            }).Returns(Task.FromResult(true));
            var service = new ShowsService(factory, SetupTvMazeServer(newShowsUpdates, newShows));

            await service.Sync();

            _showRepository.Verify(s => s.GetAll(), Times.Once);
            _showRepository.Verify(s => s.Add(It.IsAny<IEnumerable<Show>>()), Times.Once);
            _showRepository.Verify(s => s.Update(It.IsAny<IEnumerable<Show>>()), Times.Never);
            _showRepository.Verify(s => s.Remove(It.IsAny<IEnumerable<Show>>()), Times.Once);
        }
    }
}
