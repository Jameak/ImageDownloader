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
    public class ImgurAccountImagesSourceTest
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

            var source = new ImgurAccountImagesSource(null, mock.Object);
            var result = await source.GetContent("test");

            Assert.NotNull(result);
            Assert.Empty(result.Images);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_when_ratelimiter_allows_request_given_nonexistant_username_returns_empty_album()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var username = "example";
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/count"), HttpStatusCode.NotFound, new object());

            var source = new ImgurAccountImagesSource(StubHttpClient.Create(handler), mock.Object);
            var result = await source.GetContent(username);

            Assert.NotNull(result);
            Assert.Empty(result.Images);
            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_username_with_1_image_of_valid_type_returns_album_with_1_image()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);
            
            var username = "example";
            var image = new ImgurImage {Height = 10, Width = 5, Title = "title", Type = "image/jpg"};
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/count"), HttpStatusCode.OK, new ApiHelper<int> {Data = 1});
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/0"), HttpStatusCode.OK, new ApiHelper<ICollection<ImgurImage>> {Data = new List<ImgurImage> {image} });

            var source = new ImgurAccountImagesSource(StubHttpClient.Create(handler), mock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(username);

            Assert.NotNull(result);
            Assert.Equal(1, result.Images.Count);
            Assert.Equal(image.Height, await result.Images.First().GetHeight());
            Assert.Equal(image.Width, await result.Images.First().GetWidth());
            Assert.Equal(".jpg", await result.Images.First().GetImageType());

            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Exactly(2));
        }

        [Fact]
        public async void GetContent_given_username_with_1_image_of_invalid_type_returns_empty_album()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var username = "example";
            var image = new ImgurImage { Height = 10, Width = 5, Title = "title", Type = "image/png" };
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/count"), HttpStatusCode.OK, new ApiHelper<int> { Data = 1 });
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/0"), HttpStatusCode.OK, new ApiHelper<ICollection<ImgurImage>> { Data = new List<ImgurImage> { image } });

            var source = new ImgurAccountImagesSource(StubHttpClient.Create(handler), mock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(username);

            Assert.NotNull(result);
            Assert.Empty(result.Images);

            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.Exactly(2));
        }

        [Fact]
        public async void GetContent_given_username_with_160_images_of_valid_type_returns_album_with_160_image()
        {
            var mock = new Mock<ImgurRatelimiter>(null);
            mock.Setup(m => m.IsRequestAllowed()).Returns(true);
            mock.Setup(m => m.LimitsHaveBeenLoaded()).Returns(true);

            var username = "example";
            var num = 160;
            var list50 = new List<ImgurImage>(Enumerable.Repeat(new ImgurImage {Type = "image/jpg"}, 50));
            var list10 = new List<ImgurImage>(Enumerable.Repeat(new ImgurImage {Type = "image/jpg"}, 10));
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/count"), HttpStatusCode.OK, new ApiHelper<int> { Data = num });
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/0"), HttpStatusCode.OK, new ApiHelper<ICollection<ImgurImage>> { Data = list50 });
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/1"), HttpStatusCode.OK, new ApiHelper<ICollection<ImgurImage>> { Data = list50 });
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/2"), HttpStatusCode.OK, new ApiHelper<ICollection<ImgurImage>> { Data = list50 });
            handler.AddResponse(new Uri($"https://api.imgur.com/3/account/{username}/images/3"), HttpStatusCode.OK, new ApiHelper<ICollection<ImgurImage>> { Data = list10 });

            var source = new ImgurAccountImagesSource(StubHttpClient.Create(handler), mock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(username);

            Assert.NotNull(result);
            Assert.Equal(num, result.Images.Count);

            mock.Verify(i => i.LimitsHaveBeenLoaded(), Times.Once);
            mock.Verify(i => i.IsRequestAllowed(), Times.AtLeastOnce);
        }
    }
}
