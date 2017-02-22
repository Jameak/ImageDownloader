using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Responses.Impl
{
    public class ImgurAlbum : IApiCollection<ImgurImage>
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Account_url { get; set; }
        public int? Account_id { get; set; }
        public string Link { get; set; }
        public bool? Nsfw { get; set; }
        public int Images_count { get; set; }
        public ICollection<ImgurImage> Images { get; set; } = new List<ImgurImage>();

        /// <summary>
        /// See <see cref="IApiCollection{T}.GetImages()"/>
        /// </summary>
        public IEnumerable<ImgurImage> GetImages()
        {
            return Images;
        }

        public async Task RemoveNonsupportedImages()
        {
            var newCollection = new List<ImgurImage>();
            foreach (var image in Images)
            {
                var ext = await image.GetImageType();
                if (Settings.GetSupportedExtensions().Contains(ext.ToLower()))
                {
                    newCollection.Add(image);
                }
            }

            Images = newCollection;
        }
    }
}
