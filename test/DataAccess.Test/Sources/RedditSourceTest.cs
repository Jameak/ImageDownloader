using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.OAuth;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Moq;
using Xunit;

namespace DataAccess.Test.Sources
{
    public class RedditSourceTest
    {
        public static ApiHelper<ApiHelper<ApiHelper<RedditPost>>> CreateApiReturnValue(params RedditPost[] posts)
        {
            return new ApiHelper<ApiHelper<ApiHelper<RedditPost>>>
            {
                Data = new ApiHelper<ApiHelper<RedditPost>>
                {
                    Children = posts.Select(post => new ApiHelper<RedditPost> { Data = post }).ToList()
                }
            };
        }

        public static ISettingsManager CreateSettings()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetSupportedExtensions()).Returns(new StringCollection { ".jpg" });
            return mock.Object;
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_a_selfpost_returns_empty_album()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken {Access_token = "test"});
            
            var query = "example.json";
            var returnedInfo = CreateApiReturnValue(new RedditPost {Is_self = true});

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, null, null, tokenMock.Object);
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Empty(result.GetImages());
            Assert.Empty(result.GetCollections());
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_link_to_deviantart_returns_album_with_image()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });

            var url = "example.deviantart.com/test";
            var deviantMock = new Mock<ISource<DeviantartImage>>();
            deviantMock.Setup(m => m.GetContent(url)).ReturnsAsync(new DeviantartImage {Title = "deviant title"});
            
            var query = "example.json";
            var returnedInfo =
                CreateApiReturnValue(new RedditPost
                {
                    Domain = "example.deviantart.com",
                    Url = url
                });

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, null, deviantMock.Object, tokenMock.Object);
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.GetImages().Count());
            Assert.NotNull(result.GetImages().First() as DeviantartImage);
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
            deviantMock.Verify(i => i.GetContent(url), Times.Once);
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_link_to_imgur_image_returns_album_with_image()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });

            var imageId = "test";
            var url = $"imgur.com/{imageId}";
            var imgurMock = new Mock<ISource<ImgurImage>>();
            imgurMock.Setup(m => m.GetContent(imageId)).ReturnsAsync(new ImgurImage { Title = "imgur title" });

            var query = "example.json";
            var returnedInfo =
                CreateApiReturnValue(new RedditPost
                {
                    Domain = "imgur.com",
                    Url = url
                });

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, imgurMock.Object, null, tokenMock.Object);
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.GetImages().Count());
            Assert.NotNull(result.GetImages().First() as ImgurImage);
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
            imgurMock.Verify(i => i.GetContent(imageId), Times.Once);
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_link_to_imgur_album_returns_album_with_images()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });

            var albumId = "test";
            var url = $"imgur.com/a/{albumId}";
            var imgurMock = new Mock<ISource<ImgurAlbum>>();
            imgurMock.Setup(m => m.GetContent(albumId)).ReturnsAsync(new ImgurAlbum {Images = new List<ImgurImage> {new ImgurImage()} });

            var query = "example.json";
            var returnedInfo =
                CreateApiReturnValue(new RedditPost
                {
                    Domain = "imgur.com",
                    Url = url
                });

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, imgurMock.Object, null, null, tokenMock.Object);
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.GetImages().Count());
            Assert.NotNull(result.GetImages().First() as ImgurImage);
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
            imgurMock.Verify(i => i.GetContent(albumId), Times.Once);
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_link_to_reddit_image_service_returns_album_with_image()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });

            var url = "i.redd.it/test.jpg";

            var query = "example.json";
            var returnedInfo =
                CreateApiReturnValue(new RedditPost
                {
                    Domain = "i.redd.it",
                    Url = url
                });

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, null, null, tokenMock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.GetImages().Count());
            Assert.NotNull(result.GetImages().First() as GenericImage);
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_link_containing_extension_but_nonsupported_domain_returns_album_with_image()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });

            var url = "example.com/test.jpg";

            var query = "example.json";
            var returnedInfo =
                CreateApiReturnValue(new RedditPost
                {
                    Domain = "example.com",
                    Url = url
                });

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, null, null, tokenMock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.GetImages().Count());
            Assert.NotNull(result.GetImages().First() as GenericImage);
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
        }

        [Fact]
        public async void GetContent_given_url_for_listing_with_link_containing_nonsupported_extension_to_nonsupported_domain_returns_empty_album()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });

            var url = "example.com/test.what";

            var query = "example.json";
            var returnedInfo =
                CreateApiReturnValue(new RedditPost
                {
                    Domain = "example.com",
                    Url = url
                });

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=1"), HttpStatusCode.OK, returnedInfo);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, null, null, tokenMock.Object);
            source.Settings = CreateSettings();
            var result = await source.GetContent(query, 1);

            Assert.NotNull(result);
            Assert.Empty(result.GetImages());
            tokenMock.Verify(i => i.AcquireToken(), Times.Once);
        }

        [Fact]
        public async void GetContent_query_large_number_of_varying_posts()
        {
            var tokenMock = new Mock<ITokenAcquirer<RedditToken>>();
            tokenMock.Setup(m => m.AcquireToken()).ReturnsAsync(new RedditToken { Access_token = "test" });
            
            var imgurMock = new Mock<ISource<ImgurImage>>();
            imgurMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(new ImgurImage());

            var return1 = CreateApiReturnValue(Enumerable.Repeat(new RedditPost {Domain = "imgur.com", Url = "imgur.com/test"}, 100).ToArray());
            return1.Data.After = nameof(return1);
            var return2 = CreateApiReturnValue(Enumerable.Repeat(new RedditPost {Is_self = true}, 100).ToArray());
            return2.Data.After = nameof(return2);
            var return3 = CreateApiReturnValue(Enumerable.Repeat(new RedditPost { Domain = "i.imgur.com", Url = "i.imgur.com/test" }, 40).ToArray());

            var query = "example.json";
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=100"), HttpStatusCode.OK, return1);
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=100&after={nameof(return1)}&count=100"), HttpStatusCode.OK, return2);
            handler.AddResponse(new Uri($"https://oauth.reddit.com/r/{query}?limit=40&after={nameof(return2)}&count=200"), HttpStatusCode.OK, return3);
            var client = StubHttpClient.Create(handler);

            var source = new RedditSource(client, null, imgurMock.Object, null, tokenMock.Object);
            var result = await source.GetContent(query, 240);

            Assert.NotNull(result);
            Assert.Equal(140, result.GetImages().Count());
            imgurMock.Verify(i => i.GetContent(It.IsAny<string>()), Times.Exactly(140));
            tokenMock.Verify(i => i.AcquireToken(), Times.AtLeast(1));
        }
    }
}
