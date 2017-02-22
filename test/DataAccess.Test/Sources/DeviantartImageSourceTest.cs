using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Moq;
using Xunit;

namespace DataAccess.Test.Sources
{
    public class DeviantartImageSourceTest
    {
        public static ISettingsManager CreateSettings()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetSupportedExtensions()).Returns(new StringCollection{".jpg"});
            return mock.Object;
        }

        [Fact]
        public async void GetContent_given_nonexistant_url_returns_null()
        {
            var testUrl = "example.deviantart.com/testtesttest";
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"http://backend.deviantart.com/oembed?url={testUrl}"), HttpStatusCode.NotFound, new object());
            
            var source = new DeviantartImageSource(StubHttpClient.Create(handler));
            var result = await source.GetContent(testUrl);

            Assert.Null(result);
        }

        [Fact]
        public async void GetContent_given_valid_url_of_valid_imageformat_returns_valid_image()
        {
            var testUrl = "example.deviantart.com/testtesttest.jpg";
            var output = new DeviantartImage {Author_name = "example", Height = 5, Width = 10, Title = "test", Url = testUrl };

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"http://backend.deviantart.com/oembed?url={testUrl}"), HttpStatusCode.OK, output);
            
            var source = new DeviantartImageSource(StubHttpClient.Create(handler));
            source.Settings = CreateSettings();
            var result = await source.GetContent(testUrl);

            Assert.Equal(output.Author_name, result.Author_name);
            Assert.Equal(output.Title, result.Title);
            Assert.Equal(output.Height, result.Height);
            Assert.Equal(output.Width, result.Width);
            Assert.Equal(output.Url, result.Url);
        }

        [Fact]
        public async void GetContent_given_valid_url_of_invalid_imageformat_returns_null()
        {
            var testUrl = "example.deviantart.com/testtesttest.ini";
            var output = new DeviantartImage { Author_name = "example", Height = 5, Width = 10, Title = "test", Url = testUrl };

            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri($"http://backend.deviantart.com/oembed?url={testUrl}"), HttpStatusCode.OK, output);

            var source = new DeviantartImageSource(StubHttpClient.Create(handler));
            source.Settings = CreateSettings();
            var result = await source.GetContent(testUrl);

            Assert.Null(result);
        }
    }
}
