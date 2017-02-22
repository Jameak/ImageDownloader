using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Xunit;

namespace DataAccess.Test.Sources
{
    public class ImgurRatelimitSourceTest
    {
        [Fact]
        public async void GetContent_success_returns_ratelimit_response()
        {
            var response = new ImgurRatelimitResponse {ClientRemaining = 10, ClientLimit = 20, UserLimit = 30, UserRemaining = 40, UserReset = 50};
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri("https://api.imgur.com/3/credits"), HttpStatusCode.OK, new ApiHelper<ImgurRatelimitResponse> {Data = response});

            var source = new ImgurRatelimitSource(StubHttpClient.Create(handler));
            var result = await source.GetContent();

            Assert.NotNull(result);
            Assert.Equal(response.ClientRemaining, result.ClientRemaining);
            Assert.Equal(response.ClientLimit, result.ClientLimit);
            Assert.Equal(response.UserLimit, result.UserLimit);
            Assert.Equal(response.UserRemaining, result.UserRemaining);
            Assert.Equal(response.UserReset, result.UserReset);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async void GetContent_when_connection_forbidden_returns_response_with_statuscode()
        {
            var response = new ImgurRatelimitResponse();
            var handler = StubHttpClient.GetHandler();
            handler.AddResponse(new Uri("https://api.imgur.com/3/credits"), HttpStatusCode.Forbidden, new ApiHelper<ImgurRatelimitResponse> { Data = response });

            var source = new ImgurRatelimitSource(StubHttpClient.Create(handler));
            var result = await source.GetContent();

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }
    }
}
