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
    public class ImgurImageSourceTest
    {
        public static ISettingsManager CreateSettings()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetSupportedExtensions()).Returns(new StringCollection { ".jpg" });
            return mock.Object;
        }

        [Fact]
        public async void GetContent_given_nonexistant_url_returns_null()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);
            
            var imageId = "example";
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/image/{imageId}"), HttpStatusCode.NotFound, new object());

            var source = new ImgurImageSource(StubHttpClient.Create(handler), mock.Object);
            var result = await source.GetContent(imageId);

            Assert.Null(result);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_valid_url_of_valid_imageformat_returns_valid_image()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var imageId = "example";
            var output = new ImgurImage { Height = 5, Width = 10, Title = "test", Link = $"i.imgur.com/{imageId}", Type = "image/jpg"};

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/image/{imageId}"), HttpStatusCode.OK, new ApiHelper<ImgurImage> {Data = output});

            var source = new ImgurImageSource(StubHttpClient.Create(handler), mock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(imageId);
            
            Assert.Equal(output.Title, result.Title);
            Assert.Equal(output.Height, result.Height);
            Assert.Equal(output.Width, result.Width);
            Assert.Equal(output.Link, result.Link);
            Assert.Equal(output.Type, result.Type);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_valid_url_of_invalid_imageformat_returns_null()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var imageId = "example";
            var output = new ImgurImage { Height = 5, Width = 10, Title = "test", Link = $"i.imgur.com/{imageId}", Type = "image/png" };

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/image/{imageId}"), HttpStatusCode.OK, new ApiHelper<ImgurImage> {Data = output});

            var source = new ImgurImageSource(StubHttpClient.Create(handler), mock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(imageId);

            Assert.Null(result);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_when_ratelimiter_refuses_request_returns_empty_album()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(false);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var source = new ImgurImageSource(null, mock.Object);
            var result = await source.GetContent("example");

            Assert.Null(result);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }
    }
}
