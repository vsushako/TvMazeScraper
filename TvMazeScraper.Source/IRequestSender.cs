﻿using System.Threading.Tasks;

namespace TvMazeScraper.Source
{
    /// <summary>
    /// Service to communicate to different web sources
    /// </summary>
    public interface IRequestSender
    {
        /// <summary>
        /// Server Address
        /// </summary>
        string Address { get; set; }
        
        /// <summary>
        /// Method gets data from source
        /// </summary>
        /// <param name="url">Url destination</param>
        /// <returns></returns>
        Task<string> Get(string url);
    }
}
