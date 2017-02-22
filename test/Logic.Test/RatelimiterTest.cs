using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using Moq;
using Xunit;

namespace Logic.Test
{
    public class RatelimiterTest
    {
        [Fact]
        public async void GetImgurLimitInfo_calls_LoadLimits_and_returns_them()
        {
            var rateMock = new Mock<ImgurRatelimiter>(null);

            var limitTuple = new Tuple<int,int,int,int>(200, 150, 100, 50);
            rateMock.Setup(m => m.GetLimiterValues()).Returns(limitTuple);

            var ratelimiter = new Ratelimiter(rateMock.Object);
            var result = await ratelimiter.GetImgurLimitInfo();

            Assert.Equal(limitTuple.Item1, result.Item1);
            Assert.Equal(limitTuple.Item2, result.Item2);
            Assert.Equal(limitTuple.Item3, result.Item3);
            Assert.Equal(limitTuple.Item4, result.Item4);
            rateMock.Verify(i => i.LoadLimits(), Times.Once);
        }
    }
}
