using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using DataAccess.Sources;

namespace Logic.Handlers
{
    public class LocalHandler : AbstractHandler<LocalHandler.LocalFilter, LocalDirectory>
    {
        private readonly ISource<LocalDirectory> _source;

        public LocalHandler(ISource<LocalDirectory> source)
        {
            _source = source;
        }

        /// <summary>
        /// <see cref="IHandler{T,K}.ParseSource(string,bool,Nullable{int})"/>
        /// </summary>
        /// <param name="source"><see cref="ISource{T}.GetContent(string)"/></param>
        /// <param name="allowNestedCollections">Not supported parameter</param>
        /// <param name="amount">Not supported parameter</param>
        /// <returns></returns>
        public override async Task<LocalDirectory> ParseSource(string source, bool allowNestedCollections = true, int? amount = null)
        {
            return await _source.GetContent(source);
        }

        /// <summary>
        /// <see cref="IHandler{T,K}.FetchContent(x,string,x,ICollection{string})"/>
        /// </summary>
        public override async Task FetchContent(LocalDirectory parsedSource, string targetFolder, LocalFilter filter, ICollection<string> outputLog)
        {
            await Task.Run(async () =>
            {
                if (parsedSource.GetImages() != null)
                {
                    outputLog.Add($"Starting filtering of {parsedSource.GetImages().Count()} images.");

                    var sync = new object();

                    foreach (var image in parsedSource.GetImages().Where(image => image != null).AsParallel())
                    {
                        var imageName = Filenamer.Clean(await image.GetImageName());

                        try
                        {
                            if (filter(await image.GetHeight(), await image.GetWidth(), await image.GetAspectRatio()))
                            {
                                var path = Filenamer.DetermineUniqueFilename(Path.Combine(targetFolder, imageName));

                                File.WriteAllBytes(path, await image.GetImage());

                                lock (sync)
                                {
                                    outputLog.Add($"Image copied: {imageName}");
                                    image.Dispose();
                                }
                            }
                            else
                            {
                                lock (sync)
                                {
                                    outputLog.Add($"Image skipped: {imageName}");
                                    image.Dispose();
                                }
                            }
                        }
                        catch (IOException)
                        {
                            lock (sync)
                            {
                                outputLog.Add($"IO Failure - Error occured while saving or loading image: {imageName}");
                                image.Dispose();
                            }
                        }
                    }
                }

                outputLog.Add("Filtering finished.");
            });
        }

        public delegate bool LocalFilter(int imageHeight, int imageWidth, Tuple<int, int> aspectRatio);
    }
}
