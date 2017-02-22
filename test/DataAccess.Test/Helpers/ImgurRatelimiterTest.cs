using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DataAccess.Exceptions;
using DataAccess.Helpers;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Moq;
using Xunit;

namespace DataAccess.Test.Helpers
{
    public class ImgurRatelimiterTest
    {
        [Fact]
        public async void LoadLimits_given_successful_ratelimitresponse_sets_values()
        {
            var limitSourceMock = new Mock<ISource<ImgurRatelimitResponse>>();

            var limitResponse = new ImgurRatelimitResponse {ClientLimit = 10, ClientRemaining = 20, UserLimit = 30, UserRemaining = 40, StatusCode = HttpStatusCode.OK};
            limitSourceMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(limitResponse);

            var source = new ImgurRatelimiter(limitSourceMock.Object);
            await source.LoadLimits();

            var vals = source.GetLimiterValues();
            Assert.Equal(limitResponse.ClientLimit, vals.Item1);
            Assert.Equal(limitResponse.UserLimit, vals.Item2);
            Assert.Equal(limitResponse.ClientRemaining, vals.Item3);
            Assert.Equal(limitResponse.UserRemaining, vals.Item4);
            limitSourceMock.Verify(i => i.GetContent(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void LoadLimits_when_connection_forbidden_sets_expected_values()
        {
            var limitSourceMock = new Mock<ISource<ImgurRatelimitResponse>>();

            var limitResponse = new ImgurRatelimitResponse { StatusCode = HttpStatusCode.Forbidden };
            limitSourceMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(limitResponse);

            var source = new ImgurRatelimiter(limitSourceMock.Object);

            await Assert.ThrowsAsync<InvalidClientIDException>(source.LoadLimits);
            
            var vals = source.GetLimiterValues();
            Assert.Equal(-1, vals.Item1);
            Assert.Equal(-1, vals.Item2);
            Assert.Equal(-1, vals.Item3);
            Assert.Equal(-1, vals.Item4);
            limitSourceMock.Verify(i => i.GetContent(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void IsRequestAllowed_when_limits_arent_hit_returns_true()
        {
            var limitSourceMock = new Mock<ISource<ImgurRatelimitResponse>>();

            var limitResponse = new ImgurRatelimitResponse { ClientLimit = 12500, ClientRemaining = 12090, UserLimit = 500, UserRemaining = 450, StatusCode = HttpStatusCode.OK };
            limitSourceMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(limitResponse);

            var source = new ImgurRatelimiter(limitSourceMock.Object);
            await source.LoadLimits();

            Assert.True(source.IsRequestAllowed());
        }

        [Fact]
        public async void IsRequestAllowed_when_limists_are_hit_returns_false()
        {
            var limitSourceMock = new Mock<ISource<ImgurRatelimitResponse>>();

            var limitResponse = new ImgurRatelimitResponse { ClientLimit = 12500, ClientRemaining = 0, UserLimit = 500, UserRemaining = 0, StatusCode = HttpStatusCode.OK };
            limitSourceMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(limitResponse);

            var source = new ImgurRatelimiter(limitSourceMock.Object);
            await source.LoadLimits();

            Assert.False(source.IsRequestAllowed());
        }

        [Fact]
        public async void UpdateLimit_given_headers_containing_decreased_values_updates_limits()
        {
            var limitSourceMock = new Mock<ISource<ImgurRatelimitResponse>>();

            var limitResponse = new ImgurRatelimitResponse { ClientLimit = 12500, ClientRemaining = 12000, UserLimit = 500, UserRemaining = 400, StatusCode = HttpStatusCode.OK };
            limitSourceMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(limitResponse);

            var source = new ImgurRatelimiter(limitSourceMock.Object);
            await source.LoadLimits();

            var headers = new List<KeyValuePair<string, IEnumerable<string>>>();
            headers.Add(new KeyValuePair<string, IEnumerable<string>>("X-RateLimit-ClientRemaining", Enumerable.Repeat("1100",1)));
            headers.Add(new KeyValuePair<string, IEnumerable<string>>("X-RateLimit-UserLimit", Enumerable.Repeat("300",1)));

            source.UpdateLimit(headers);
            var vals = source.GetLimiterValues();

            Assert.Equal(limitResponse.ClientLimit, vals.Item1);
            Assert.Equal(limitResponse.UserLimit, vals.Item2);
            Assert.Equal(1100, vals.Item3);
            Assert.Equal(300, vals.Item4);
        }

        [Fact]
        public async void UpdateLimits_given_headers_with_reset_limits_updates_limits()
        {
            var limitSourceMock = new Mock<ISource<ImgurRatelimitResponse>>();

            var limitResponse = new ImgurRatelimitResponse { ClientLimit = 12500, ClientRemaining = 1000, UserLimit = 500, UserRemaining = 50, StatusCode = HttpStatusCode.OK };
            limitSourceMock.Setup(m => m.GetContent(It.IsAny<string>())).ReturnsAsync(limitResponse);

            var source = new ImgurRatelimiter(limitSourceMock.Object);
            await source.LoadLimits();

            var headers = new List<KeyValuePair<string, IEnumerable<string>>>
            {
                new KeyValuePair<string, IEnumerable<string>>("X-RateLimit-ClientRemaining", Enumerable.Repeat("12490", 1)),
                new KeyValuePair<string, IEnumerable<string>>("X-RateLimit-UserLimit", Enumerable.Repeat("498", 1))
            };

            source.UpdateLimit(headers);
            var vals = source.GetLimiterValues();

            Assert.Equal(limitResponse.ClientLimit, vals.Item1);
            Assert.Equal(limitResponse.UserLimit, vals.Item2);
            Assert.Equal(12490, vals.Item3);
            Assert.Equal(498, vals.Item4);
        }
    }
}
