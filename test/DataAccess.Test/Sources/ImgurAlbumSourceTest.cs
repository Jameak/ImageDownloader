using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Moq;
using Xunit;

namespace DataAccess.Test.Sources
{
    public class ImgurAlbumSourceTest
    {
        public static ISettingsManager CreateSettings()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetSupportedExtensions()).Returns(new StringCollection { ".jpg" });
            return mock.Object;
        }

        [Fact]
        public async void GetContent_when_ratelimiter_refuses_request_returns_empty_album()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(false);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var source = new ImgurAlbumSource(null, mock.Object);
            var result = await source.GetContent("test");

            Assert.NotNull(result);
            Assert.Empty(result.Images);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_when_ratelimiter_allows_request_given_nonexistant_albumid_returns_empty_album()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var albumId = "example";
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/album/{albumId}"), HttpStatusCode.NotFound, new object());

            var source = new ImgurAlbumSource(StubHttpClient.Create(handler), mock.Object);
            var result = await source.GetContent(albumId);

            Assert.NotNull(result);
            Assert.Empty(result.Images);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_albumid_with_images_of_valid_type_returns_album_with_images()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var albumId = "example";
            var imageNum = 15;
            var album = new ImgurAlbum {Images = new List<ImgurImage>(Enumerable.Repeat(new ImgurImage {Type = "image/jpg"}, imageNum)) };
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/album/{albumId}"), HttpStatusCode.OK, new ApiHelper<ImgurAlbum> {Data = album });

            var source = new ImgurAlbumSource(StubHttpClient.Create(handler), mock.Object);
            var result = await source.GetContent(albumId);

            Assert.NotNull(result);
            Assert.NotNull(result.Images);
            Assert.Equal(imageNum, result.Images.Count);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_albumid_with_images_of_invalid_type_returns_empty_album()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var albumId = "example";
            var imageNum = 15;
            var album = new ImgurAlbum { Images = new List<ImgurImage>(Enumerable.Repeat(new ImgurImage { Type = "image/test" }, imageNum)) };
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/album/{albumId}"), HttpStatusCode.OK, new ApiHelper<ImgurAlbum> { Data = album });

            var source = new ImgurAlbumSource(StubHttpClient.Create(handler), mock.Object);
            var result = await source.GetContent(albumId);

            Assert.NotNull(result);
            Assert.NotNull(result.Images);
            Assert.Equal(0, result.Images.Count);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }
    }
}
