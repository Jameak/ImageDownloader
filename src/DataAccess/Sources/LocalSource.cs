using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Responses;
using DataAccess.Responses.Impl;

namespace DataAccess.Sources
{
    /// <summary>
    /// Provides a mechanism for finding all image-files in a local directory.
    /// </summary>
    public class LocalSource : ISource<LocalDirectory>
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// 
        /// Gets the content of the given directory and all subdirectories.
        /// </summary>
        /// <param name="directory">The directory whose content to get</param>
        public async Task<LocalDirectory> GetContent(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return new LocalDirectory {Directory = directory};
            }

            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

            return new LocalDirectory
            {
                Directory = directory,
                Images =
                    files.Where(i => Settings.GetSupportedExtensions().Contains(Path.GetExtension(i.ToLower())))
                        .Select(i => new LocalImage {ImagePath = i})
                        .ToList()
            };
        }
    }
}
