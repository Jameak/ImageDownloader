using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Sources;
using Moq;
using Xunit;

namespace DataAccess.Test.Sources
{
    public class LocalSourceTest
    {
        public static ISettingsManager CreateSettings()
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetSupportedExtensions()).Returns(new StringCollection { ".jpg" });
            return mock.Object;
        }

        [Fact]
        public async void GetContent_given_nonexistant_directory_returns_empty_album()
        {
            var dir = $"test_{GetType().Name}_{nameof(GetContent_given_empty_directory_returns_empty_album)}";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), dir);

            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }

            var source = new LocalSource();
            var result = await source.GetContent(directory);

            Assert.NotNull(result);
            Assert.Empty(result.Images);
        }

        [Fact]
        public async void GetContent_given_empty_directory_returns_empty_album()
        {
            var dir = $"test_{GetType().Name}_GetContent_empty_album";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), dir);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            //Ensure that the directory that we expect to be empty is actually empty before we do the test.
            Assert.Empty(Directory.GetFiles(directory, "*", SearchOption.AllDirectories));

            var source = new LocalSource();
            var result = await source.GetContent(directory);

            Assert.NotNull(result);
            Assert.Empty(result.Images);
        }

        [Fact]
        public async void GetContent_given_directory_returns_album_containing_supported_images_in_directory()
        {
            var dir = $"test_{GetType().Name}_GetContent_check_images";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), dir);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Create(Path.Combine(directory, "file1.png")).Dispose();
            File.Create(Path.Combine(directory, "file2.jPG")).Dispose();
            File.Create(Path.Combine(directory, "file3.jpg")).Dispose();
            File.Create(Path.Combine(directory, "file4.PNG")).Dispose();
            File.Create(Path.Combine(directory, "file5.JPG")).Dispose();

            var source = new LocalSource();
            source.Settings = CreateSettings();
            var result = await source.GetContent(directory);

            Assert.NotNull(result);
            Assert.Equal(3, result.Images.Count);
            Assert.NotNull(result.Images.FirstOrDefault(i => i.GetImageName().Result.ToLower() == "file2.jpg"));
            Assert.NotNull(result.Images.FirstOrDefault(i => i.GetImageName().Result.ToLower() == "file3.jpg"));
            Assert.NotNull(result.Images.FirstOrDefault(i => i.GetImageName().Result.ToLower() == "file5.jpg"));
        }
    }
}
