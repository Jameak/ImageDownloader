using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.Responses;
using DataAccess.Responses.Impl;
using Newtonsoft.Json;

namespace DataAccess.Sources
{
    /// <summary>
    /// Provides a mechanism for downloading album information and
    /// all images in that album.
    /// </summary>
    public class ImgurAlbumSource : ISource<ImgurAlbum>
    {
        private readonly HttpClient _client;
        private readonly ImgurRatelimiter _ratelimiter;

        /// <summary>
        /// Initializes a new instance of an ImgurAlbumSource
        /// </summary>
        /// <param name="client">A HttpClient containing the Client-ID authentication header </param>
        /// <param name="ratelimiter">The ratelimiter whose limits should be respected</param>
        public ImgurAlbumSource(HttpClient client, ImgurRatelimiter ratelimiter)
        {
            _client = client;
            _ratelimiter = ratelimiter;
        }

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// 
        /// Respects the ratelimits imposed on Imgur requests.
        /// </summary>
        /// <param name="albumId">The id of the album to get</param>
        public async Task<ImgurAlbum> GetContent(string albumId)
        {
            if (!_ratelimiter.LimitsHaveBeenLoaded()) await _ratelimiter.AttemptToLoadLimits();

            if (_ratelimiter.IsRequestAllowed())
            {
                var result = await _client.GetAsync($"https://api.imgur.com/3/album/{albumId}");
                _ratelimiter.UpdateLimit(result.Headers.ToList());

                if (result.IsSuccessStatusCode)
                {
                    var val = await result.Content.ReadAsStringAsync();
                    var album = JsonConvert.DeserializeObject<ApiHelper<ImgurAlbum>>(val);

                    await album.Data.RemoveNonsupportedImages();

                    return album.Data;
                }
            }

            return new ImgurAlbum();
        }
    }

    
}
