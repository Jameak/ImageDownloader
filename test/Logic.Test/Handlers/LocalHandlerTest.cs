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
    public class LocalHandlerTest
    {
        [Fact]
        public async void ParseSource_given_no_amount_calls_ISource_without_specifying_amount()
        {
            var sourceMock = new Mock<ISource<LocalDirectory>>();
            var input = "test";
            var mockReturn = new LocalDirectory {Images = new List<LocalImage> {new LocalImage(), new LocalImage() } };
            sourceMock.Setup(m => m.GetContent(input)).ReturnsAsync(mockReturn);

            var handler = new LocalHandler(sourceMock.Object);
            var output = await handler.ParseSource(input);

            Assert.Equal(2,output.Images.Count);
            sourceMock.Verify(i => i.GetContent(input), Times.Once);
        }
    }
}
