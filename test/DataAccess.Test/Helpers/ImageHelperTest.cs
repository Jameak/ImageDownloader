using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;
using Moq;
using Xunit;

namespace DataAccess.Test.Helpers
{
    public class ImageHelperTest
    {
        public static ISettingsManager CreateSettings()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetFallbackWidth()).Returns(1000);
            mock.Setup(m => m.GetFallbackHeight()).Returns(2000);
            return mock.Object;
        }

        [Fact]
        public void GetAspectRatio_given_reduceable_ratio_returns_reduced_ratio()
        {
            var width = 1920;
            var height = 1080;
            var ratio = ImageHelper.GetAspectRatio(width, height, CreateSettings());
            Assert.Equal(16, ratio.Item1);
            Assert.Equal(9, ratio.Item2);
        }

        [Fact]
        public void GetAspectRatio_given_unreduceable_ratio_returns_original_ratio()
        {
            var width = 1001;
            var height = 2;
            var ratio = ImageHelper.GetAspectRatio(width, height, CreateSettings());
            Assert.Equal(1001, ratio.Item1);
            Assert.Equal(2, ratio.Item2);
        }

        [Fact]
        public void GetAspectRatio_given_zeroes_returns_fallback_values()
        {
            var width = 0;
            var height = 0;
            var settings = CreateSettings();
            var ratio = ImageHelper.GetAspectRatio(width, height, settings);
            Assert.Equal(settings.GetFallbackWidth(), ratio.Item1);
            Assert.Equal(settings.GetFallbackHeight(), ratio.Item2);
        }

        [Fact]
        public void GetAspectRatio_given_zero_width_returns_fallback_values()
        {
            var width = 0;
            var height = 1080;
            var settings = CreateSettings();
            var ratio = ImageHelper.GetAspectRatio(width, height, settings);
            Assert.Equal(settings.GetFallbackWidth(), ratio.Item1);
            Assert.Equal(settings.GetFallbackHeight(), ratio.Item2);
        }

        [Fact]
        public void GetAspectRatio_given_zero_height_returns_fallback_values()
        {
            var width = 1920;
            var height = 0;
            var settings = CreateSettings();
            var ratio = ImageHelper.GetAspectRatio(width, height, settings);
            Assert.Equal(settings.GetFallbackWidth(), ratio.Item1);
            Assert.Equal(settings.GetFallbackHeight(), ratio.Item2);
        }

        [Fact]
        public void GetAspectRatio_given_negative_values_returns_ratio()
        {
            var width = -1920;
            var height = -1080;
            var ratio = ImageHelper.GetAspectRatio(width, height, CreateSettings());
            Assert.Equal(-16, ratio.Item1);
            Assert.Equal(-9, ratio.Item2);
        }
    }
}
