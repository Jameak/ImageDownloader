using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using Newtonsoft.Json;

namespace DataAccess.Sources
{
    public class GithubReleaseSource : ISource<GithubReleases>
    {
        private readonly HttpClient _client;

        public GithubReleaseSource(HttpClient client)
        {
            _client = client;
        }

        public async Task<GithubReleases> GetContent(string id)
        {
            HttpResponseMessage result;
            try
            {
                result = await _client.GetAsync("https://api.github.com/repos/jameak/imagedownloader/releases");
            }
            catch (HttpRequestException)
            {
                return new GithubReleases();
            }

            if (result != null && result.IsSuccessStatusCode)
            {
                var val = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<GithubRelease>>(val);

                return new GithubReleases {Releases = response};
            }

            return new GithubReleases();
        }
    }
}
