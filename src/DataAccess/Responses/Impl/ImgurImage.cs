using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAccess.Helpers;

namespace DataAccess.Responses.Impl
{
    public class ImgurImage : IApiImage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Gifv { get; set; }
        public string Mp4 { get; set; }
        public bool? Nsfw { get; set; }

        /// <summary>
        /// See <see cref="IApiImage.GetImage()"/>
        /// </summary>
        public async Task<byte[]> GetImage()
        {
            return await ImageHelper.GetWebContent(Link);
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
            return $".{Type.Split('/').Last()}";
        }

        /// <summary>
        /// See <see cref="IApiImage.GetImageName()"/>
        /// </summary>
        public async Task<string> GetImageName()
        {
            var name = Link.Split('/').Last();
            if (!name.Contains('.'))
            {
                name += await GetImageType();
            }

            return Title != null ? $"{Title}{await GetImageType()}" : name;
        }

        public void Dispose() { }
    }
}
