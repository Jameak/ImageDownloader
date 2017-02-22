using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using Moq;
using Xunit;

namespace DataAccess.Test.Responses
{
    public class ImgurAlbumTest
    {
        [Fact]
        public async void RemoveNonsupportedImages_removes_correct_things()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetSupportedExtensions()).Returns(new StringCollection { ".jpg" });

            var album = new ImgurAlbum
            {
                Images =
                    new List<ImgurImage>
                    {
                        new ImgurImage {Type = "image/jpg"},
                        new ImgurImage {Type = "image/png"},
                        new ImgurImage {Type = "image/gif"}
                    }
            };
            album.Settings = mock.Object;

            await album.RemoveNonsupportedImages();

            Assert.Equal(1, album.Images.Count);
            Assert.Equal("image/jpg", album.Images.First().Type);
        }
    }
}
