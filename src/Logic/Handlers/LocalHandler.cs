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
            await Task.Run(() =>
            {
                if (parsedSource.GetImages() != null)
                {
                    outputLog.Add($"Starting filtering of {parsedSource.GetImages().Count()} images.");

                    var sync = new object();
                    //Limit degree of parallelism to avoid sending too many http-requests at the same time. 
                    //  8 seems like a reasonable amount of requests to have in-flight at a time.
                    var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

                    Parallel.ForEach(parsedSource.GetImages().Where(image => image != null), parallelOptions, image =>
                    {
                        using (image)
                        {
                            var imageName = Filenamer.Clean(image.GetImageName().Result);

                            try
                            {
                                if (filter(image.GetHeight().Result, image.GetWidth().Result, image.GetAspectRatio().Result))
                                {
                                    var path = Filenamer.DetermineUniqueFilename(Path.Combine(targetFolder, imageName));

                                    File.WriteAllBytes(path, image.GetImage().Result);
                                    WriteToLog(sync, outputLog, $"Image copied: {imageName}");
                                }
                                else
                                {
                                    WriteToLog(sync, outputLog, $"Image skipped: {imageName}");
                                }
                            }
                            catch (IOException)
                            {
                                WriteToLog(sync, outputLog, $"IO Failure - Error occured while saving or loading image: {imageName}");
                            }
                        }
                    });
                }

                outputLog.Add("Filtering finished.");
            });
        }

        public delegate bool LocalFilter(int imageHeight, int imageWidth, Tuple<int, int> aspectRatio);
    }
}
