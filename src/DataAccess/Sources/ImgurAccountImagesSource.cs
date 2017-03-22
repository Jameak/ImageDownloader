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
using Newtonsoft.Json;

namespace DataAccess.Sources
{
    /// <summary>
    /// Provides a mechanism for downloading all of a Imgur users public images.
    /// A users public images are visible at https://{username}.imgur.com/all
    /// </summary>
    public class ImgurAccountImagesSource : ISource<GenericAlbum>
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private readonly HttpClient _client;
        private readonly ImgurRatelimiter _ratelimiter;

        /// <summary>
        /// Initializes a new instance of an ImgurAccountImagesSource
        /// </summary>
        /// <param name="client">A HttpClient containing the Client-ID authentication header </param>
        /// <param name="ratelimiter">The ratelimiter whose limits should be respected</param>
        public ImgurAccountImagesSource(HttpClient client, ImgurRatelimiter ratelimiter)
        {
            _client = client;
            _ratelimiter = ratelimiter;
        }

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// 
        /// Respects the ratelimits imposed on Imgur requests.
        /// </summary>
        /// <param name="username">The users username</param>
        public async Task<GenericAlbum> GetContent(string username)
        {
            if (!_ratelimiter.LimitsHaveBeenLoaded()) await _ratelimiter.AttemptToLoadLimits();

            var images = new List<IApiImage>();

            if (_ratelimiter.IsRequestAllowed())
            {
                var imageNumberResult = await _client.GetAsync($"https://api.imgur.com/3/account/{username}/images/count");
                _ratelimiter.UpdateLimit(imageNumberResult.Headers.ToList());

                if (!imageNumberResult.IsSuccessStatusCode)
                {
                    return new GenericAlbum { Images = images };
                }

                //The number of images that the user has.
                var numVal = await imageNumberResult.Content.ReadAsStringAsync();
                var numImages = JsonConvert.DeserializeObject<ApiHelper<int>>(numVal).Data;
                
                //Each page contains 50 images, so page through them if we need to.
                for (var page = 0; page < Math.Ceiling(numImages / 50.0); page++)
                {
                    if (_ratelimiter.IsRequestAllowed())
                    {
                        var result = await _client.GetAsync($"https://api.imgur.com/3/account/{username}/images/{page}");
                        _ratelimiter.UpdateLimit(result.Headers.ToList());

                        if (result.IsSuccessStatusCode)
                        {
                            var val = await result.Content.ReadAsStringAsync();
                            var valParsed = JsonConvert.DeserializeObject<ApiHelper<ICollection<ImgurImage>>>(val);

                            foreach (var image in valParsed.Data)
                            {
                                var ext = await image.GetImageType();
                                if (Settings.GetSupportedExtensions().Contains(ext.ToLower()))
                                {
                                    images.Add(image);
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return new GenericAlbum { Images = images };
        }
    }
}
