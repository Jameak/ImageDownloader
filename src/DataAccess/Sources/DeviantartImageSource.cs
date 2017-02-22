using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;

namespace DataAccess.Sources
{
    /// <summary>
    /// Provides a mechanism for getting images from the Deviantart api
    /// </summary>
    public class DeviantartImageSource : ISource<DeviantartImage>
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of DeviantartImageSource
        /// </summary>
        /// <param name="client">A HttpClient containing a User-Agent header identifying this program</param>
        public DeviantartImageSource(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// </summary>
        /// <param name="url">The uri of the image to get</param>
        public async Task<DeviantartImage> GetContent(string url)
        {
            var result = await _client.GetAsync($"http://backend.deviantart.com/oembed?url={url}");

            if (result.IsSuccessStatusCode)
            {
                var image = await result.Content.ReadAsAsync<DeviantartImage>();

                var ext = await image.GetImageType();
                if (Settings.GetSupportedExtensions().Contains(ext.ToLower()))
                {
                    return image;
                }
            }

            return null;
        }
    }
}
