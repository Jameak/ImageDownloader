using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Logic.Handlers;
using Moq;
using Xunit;

namespace Logic.Test.Handlers
{
    public class ImgurHandlerTest
    {
        [Fact]
        public async void ParseSource_given_album_link_calls_ISource_for_album()
        {
            var sourceMock = new Mock<ISource<ImgurAlbum>>();
            var input = "imgur.com/a/test";
            var albumId = "test";
            var mockReturn = new ImgurAlbum() { Images = new List<ImgurImage> { new ImgurImage(), new ImgurImage() } };
            sourceMock.Setup(m => m.GetContent(albumId)).ReturnsAsync(mockReturn);

            var handler = new ImgurHandler(sourceMock.Object, null);
            var output = await handler.ParseSource(input);

            Assert.Equal(2, output.GetImages().ToList().Count);
            sourceMock.Verify(i => i.GetContent(albumId), Times.Once);
        }

        [Fact]
        public async void ParseSource_given_account_link_calls_ISource_for_account_images()
        {
            var sourceMock = new Mock<ISource<GenericAlbum>>();
            var input = "example.imgur.com";
            var username = "example";
            var mockReturn = new GenericAlbum() { Images = new List<IApiImage> { new GenericImage(), new GenericImage() } };
            sourceMock.Setup(m => m.GetContent(username)).ReturnsAsync(mockReturn);

            var handler = new ImgurHandler(null, sourceMock.Object);
            var output = await handler.ParseSource(input);

            Assert.Equal(2, output.GetImages().ToList().Count);
            sourceMock.Verify(i => i.GetContent(username), Times.Once);
        }
    }
}
