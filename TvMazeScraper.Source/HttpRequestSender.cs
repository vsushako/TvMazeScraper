using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TvMazeScraper.Source
{
    public class HttpRequestSender: IRequestSender
    {
        public string Address { get; set; }

        public async Task<string> Get(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(Address + url);
            webRequest.ContentType = "application/json";
           
            try
            {
                // Get server responce
                var webResponse = await webRequest.GetResponseAsync();

                using (var stream = webResponse.GetResponseStream())
                {
                    if (stream == null) return string.Empty;

                    using (var reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (var stream = e.Response.GetResponseStream())
                {
                    if (stream == null) throw;

                    using (var reader = new StreamReader(stream))
                    {
                        // Creates wrapper with server error text
                        throw new Exception(reader.ReadToEnd(), e);
                    }
                }
            }
        }
    }
}
