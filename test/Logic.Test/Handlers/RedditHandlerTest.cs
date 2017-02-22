using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Logic.Handlers;
using Moq;
using Xunit;

namespace Logic.Test.Handlers
{
    public class RedditHandlerTest
    {
        [Fact]
        public async void ParseSource_given_no_amount_calls_ISource_without_specifying_amount()
        {
            var sourceMock = new Mock<ICollectionSource<RedditListing>>();
            var input = "test";
            var mockReturn = new RedditListing {Posts = new List<RedditPost> {new RedditPost {Image = new GenericImage()}, new RedditPost {Image = new GenericImage()} } };
            sourceMock.Setup(m => m.GetContent(input)).ReturnsAsync(mockReturn);

            var handler = new RedditHandler(sourceMock.Object);
            var output = await handler.ParseSource(input);

            Assert.Equal(2, output.GetCollections().ToList().Count);
            Assert.Equal(2, output.GetImages().ToList().Count);
            sourceMock.Verify(i => i.GetContent(input), Times.Once);
        }

        [Fact]
        public async void ParseSource_given_amount_calls_ISource_with_amount()
        {
            var sourceMock = new Mock<ICollectionSource<RedditListing>>();
            var input = "test";
            var amount = 100;
            var mockReturn = new RedditListing { Posts = new List<RedditPost> { new RedditPost { Image = new GenericImage() }, new RedditPost { Image = new GenericImage() } } };
            sourceMock.Setup(m => m.GetContent(input, amount)).ReturnsAsync(mockReturn);

            var handler = new RedditHandler(sourceMock.Object);
            var output = await handler.ParseSource(input, true, amount);

            Assert.Equal(2, output.GetCollections().ToList().Count);
            Assert.Equal(2, output.GetImages().ToList().Count);
            sourceMock.Verify(i => i.GetContent(input, amount), Times.Once);
        }
    }
}
