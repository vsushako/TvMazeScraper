using System;
using System.Collections.Generic;
using System.Text;

namespace TvMazeScraper.Repository.Model
{
    public class Show
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Updated { get; set; }
        
        public int ExternalId { get; set; }

        public string Name { get; set; }

        public IEnumerable<Cast> Cast { get; set; }
    }
}
