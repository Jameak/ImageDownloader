using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAccess.Helpers;

namespace DataAccess.Responses.Impl
{
    /// <summary>
    /// Represents images hosted on domains that aren't supported through their API,
    /// but where it is possible to grab the image directly based on their url.
    /// </summary>
    public class GenericImage : IApiImage
    {
        public string Url { get; set; }
        private byte[] ImageArray { get; set; }
        private Image Image { get; set; }
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        /// <summary>
        /// See <see cref="IApiImage.GetImage()"/>
        /// </summary>
        public async Task<byte[]> GetImage()
        {
            return ImageArray != null ? ImageArray : ImageArray = await ImageHelper.GetWebContent(Url);
        }

        public async Task<int> GetWidth()
        {
            if (Image != null) return Image.Width;

            //return fallback value if we cant create the image, due to invalid image format or internet issues.
            return await CreateImage() ? Image.Width : Settings.GetFallbackWidth();
        }

        public async Task<int> GetHeight()
        {
            if (Image != null) return Image.Height;

            //return fallback value if we cant create the image, due to invalid image format or internet issues.
            return await CreateImage() ? Image.Height : Settings.GetFallbackHeight();
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
            return Url.Split('/').Last();
        }

        private async Task<bool> CreateImage()
        {
            if (ImageArray == null)
            {
                ImageArray = await GetImage();
            }

            if (ImageArray == null) return false;

            try
            {
                using (var ms = new MemoryStream(ImageArray))
                {
                    Image = Image.FromStream(ms);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                //Thrown if the stream does not have a valid image format
                return false;
            }
        }

        public async Task<Tuple<int, int>> GetAspectRatio()
        {
            return ImageHelper.GetAspectRatio(await GetWidth(), await GetHeight());
        }

        public void Dispose()
        {
            Image?.Dispose();
            ImageArray = null;
            Image = null;
        }
    }
}
