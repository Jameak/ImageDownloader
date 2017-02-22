using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.Responses;
using DataAccess.Responses.Impl;

namespace DataAccess.Sources
{
    public class ImgurImageSource : ISource<ImgurImage>
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private readonly HttpClient _client;
        private readonly ImgurRatelimiter _ratelimiter;

        /// <summary>
        /// Initializes a new instance of an ImgurImageSource
        /// </summary>
        /// <param name="client">A HttpClient containing the Client-ID authentication header </param>
        /// <param name="ratelimiter">The ratelimiter whose limits should be respected</param>
        public ImgurImageSource(HttpClient client, ImgurRatelimiter ratelimiter)
        {
            _client = client;
            _ratelimiter = ratelimiter;
        }

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// 
        /// Respects the ratelimits imposed on Imgur requests.
        /// </summary>
        /// <param name="imageId">The id of the image to get</param>
        public async Task<ImgurImage> GetContent(string imageId)
        {
            if (!_ratelimiter.LimitsHaveBeenLoaded()) await _ratelimiter.AttemptToLoadLimits();

            if (_ratelimiter.IsRequestAllowed())
            {
                var result = await _client.GetAsync($"https://api.imgur.com/3/image/{imageId}");
                _ratelimiter.UpdateLimit(result.Headers.ToList());

                if (result.IsSuccessStatusCode)
                {
                    var val = await result.Content.ReadAsAsync<ApiHelper<ImgurImage>>();

                    var ext = await val.Data.GetImageType();
                    if (Settings.GetSupportedExtensions().Contains(ext.ToLower()))
                    {
                        return val.Data;
                    }
                }
            }
            
            return null;
        }
    }
}
