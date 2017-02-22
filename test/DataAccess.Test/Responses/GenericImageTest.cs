using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using Xunit;

namespace DataAccess.Test.Responses
{
    public class GenericImageTest
    {
        [Fact]
        public async void GetImageType_returns_correct_type()
        {
            var image = new GenericImage { Url = "example.com/test.jpg" };
            Assert.Equal(".jpg", await image.GetImageType());
        }

        [Fact]
        public async void GetImageName_returns_correct_name()
        {
            var image = new GenericImage { Url = "example.com/test.jpg"};
            Assert.Equal("test.jpg", await image.GetImageName());
        }
    }
}
