using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.Responses.Impl;
using Newtonsoft.Json;

namespace DataAccess.Sources
{
    public class ImgurRatelimitSource : ISource<ImgurRatelimitResponse>
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of an ImgurRatelimitSource
        /// </summary>
        /// <param name="client">A HttpClient containing the Client-ID authentication header </param>
        public ImgurRatelimitSource(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// </summary>
        /// <param name="id"> Parameter is not used. The program to get ratelimit-information
        /// for is identified by the Client-ID authentication header </param>
        public async Task<ImgurRatelimitResponse> GetContent(string id = null)
        {
            var result = await _client.GetAsync("https://api.imgur.com/3/credits");
            if (result == null) return null;

            if (result.IsSuccessStatusCode)
            {
                var val = await result.Content.ReadAsStringAsync();
                var ratelimit = JsonConvert.DeserializeObject<ApiHelper<ImgurRatelimitResponse>>(val);
                ratelimit.Data.StatusCode = result.StatusCode;
                return ratelimit.Data;
            }

            return new ImgurRatelimitResponse {StatusCode = result.StatusCode};
        }
    }
}
