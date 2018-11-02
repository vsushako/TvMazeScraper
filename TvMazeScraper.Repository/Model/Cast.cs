using System;

namespace TvMazeScraper.Repository.Model
{
    public class Cast
    {
        public Guid Id { get; set; }

        public DateTime Updated { get; set; }

        public int ExternalId { get; set; }

        public string Name { get; set; }

        public DateTime Birthday { get; set; }
    }
}