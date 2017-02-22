using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Logic
{
    public static class Filenamer
    {
        private static readonly Regex Regex = new Regex(@"[^a-zA-Z0-9 \.-]");

        /// <summary>
        /// Strips all characters in the given string that are disallowed in filenames.
        /// </summary>
        public static string Clean(string filename)
        {
            return Regex.Replace(filename, "");
        }

        /// <summary>
        /// Given the path to a file, this method provides functionality for changing the
        /// given path to ensure that it is unique and will not cause any files to be
        /// overwritten, by appending a number to the filename.
        /// </summary>
        /// <param name="path">The path for which to determine a unique path</param>
        /// <returns>A path that doens't point to an existing file.</returns>
        public static string DetermineUniqueFilename(string path)
        {
            if (path == null) return null;

            var result = path;

            int i = 1;
            while (File.Exists(result))
            {
                string fileNameWithPathWithoutExtension;
                var dir = Path.GetDirectoryName(path);
                if (dir != null)
                {
                    fileNameWithPathWithoutExtension = Path.Combine(Path.GetDirectoryName(path),
                        Path.GetFileNameWithoutExtension(path));
                }
                else
                {
                    fileNameWithPathWithoutExtension = Path.GetFileNameWithoutExtension(path);
                }

                result = $"{fileNameWithPathWithoutExtension} ({i}){Path.GetExtension(path)}";
                i++;
            }

            return result;
        }
    }
}
