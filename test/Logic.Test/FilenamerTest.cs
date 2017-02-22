using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Logic.Test
{
    public class FilenamerTest
    {
        [Fact]
        public void Clean_given_string_containing_invalid_characters_returns_cleaned_string()
        {
            var input = $"test {new string(Path.GetInvalidFileNameChars())}{new string(Path.GetInvalidPathChars())}";
            var output = Filenamer.Clean(input);

            Assert.Equal("test ", output);
        }

        [Fact]
        public void DetermineUniqueFilename_given_unique_filename_returns_identical_filename()
        {
            var dir = $"test_{GetType().Name}_DetermineUniqueFilename";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), dir);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, "unique.test");
            var output = Filenamer.DetermineUniqueFilename(path);
            Assert.Equal(path, output);
        }

        [Fact]
        public void DetermineUniqueFilename_given_duplicate_filename_returns_unique_filename()
        {
            var dir = $"test_{GetType().Name}_DetermineUniqueFilename";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), dir);

            var inputname = "notunique.jpg";
            var expectedname = "notunique (2).jpg";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(Path.Combine(directory, expectedname)))
            {
                File.Delete(Path.Combine(directory, expectedname));
            }
            
            File.Create(Path.Combine(directory, "notunique.jpg")).Dispose();
            File.Create(Path.Combine(directory, "notunique (1).jpg")).Dispose();

            var output = Filenamer.DetermineUniqueFilename(Path.Combine(directory, inputname));
            Assert.Equal(Path.Combine(directory, expectedname), output);
        }
    }
}
