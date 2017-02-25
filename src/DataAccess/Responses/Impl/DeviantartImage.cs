using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;

namespace DataAccess.Responses.Impl
{
    public class DeviantartImage : IApiImage
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Author_name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void Dispose() { }

        /// <summary>
        /// See <see cref="IApiImage.GetImage()"/>
        /// </summary>
        public async Task<byte[]> GetImage()
        {
            return await ImageHelper.GetWebContent(Url);
        }

        public async Task<int> GetWidth()
        {
            return Width;
        }

        public async Task<int> GetHeight()
        {
            return Height;
        }

        /// <summary>
        /// See <see cref="IApiImage.GetImageType()"/>
        /// </summary>
        public async Task<string> GetImageType()
        {
            return $".{Url.Split('.').Last()}";
        }

        /// <summary>
        /// See <see cref="IApiImage.GetImageName()"/>
        /// </summary>
        public async Task<string> GetImageName()
        {
            return $"{Title}{await GetImageType()}";
        }

        public async Task<Tuple<int, int>> GetAspectRatio()
        {
            return ImageHelper.GetAspectRatio(await GetWidth(), await GetHeight());
        }
    }
}
