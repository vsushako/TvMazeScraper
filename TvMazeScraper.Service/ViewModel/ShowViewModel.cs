using System.Collections.Generic;
using TvMazeScraper.Service.Model;

namespace TvMazeScraper.Service.ViewModel
{
    public struct ShowOutViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<CastViewModel> Cast { get; set; }
    }
}
