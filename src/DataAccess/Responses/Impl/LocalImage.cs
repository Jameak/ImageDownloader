using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Helpers;

namespace DataAccess.Responses.Impl
{
    public class LocalImage : IApiImage
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private byte[] ImageArray { get; set; }
        private Image Image { get; set; }

        public string ImagePath { get; set; }

        /// <summary>
        /// See <see cref="IApiImage.GetImage()"/>
        /// </summary>
        public async Task<byte[]> GetImage()
        {
            if (ImageArray != null) return ImageArray;
            if (ImagePath == null) return null;
            
            return ImageArray = File.ReadAllBytes(ImagePath);
        }

        public async Task<int> GetWidth()
        {
            if (Image != null) return Image.Width;

            return await CreateImage() ? Image.Width : Settings.GetFallbackWidth(); //return fallback value if we cant create the image, due to invalid image format.
        }

        public async Task<int> GetHeight()
        {
            if (Image != null) return Image.Height;

            return await CreateImage() ? Image.Height : Settings.GetFallbackWidth(); //return fallback value if we cant create the image, due to invalid image format.
        }

        /// <summary>
        /// See <see cref="IApiImage.GetImageType()"/>
        /// </summary>
        public async Task<string> GetImageType()
        {
            return Path.GetExtension(ImagePath);
        }

        /// <summary>
        /// See <see cref="IApiImage.GetImageName()"/>
        /// </summary>
        public async Task<string> GetImageName()
        {
            return Path.GetFileName(ImagePath);
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
