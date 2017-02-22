using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Responses.Impl;
using DataAccess.Sources;

namespace Logic.Handlers
{
    public class RedditHandler : AbstractHandler<RedditHandler.RedditFilter, RedditListing>
    {
        private readonly ICollectionSource<RedditListing> _source;

        public RedditHandler(ICollectionSource<RedditListing> source)
        {
            _source = source;
        }
        
        /// <summary>
        /// <see cref="IHandler{T,K}.ParseSource(string,bool,Nullable{int})"/>
        /// </summary>
        /// <param name="source"><see cref="ISource{T}.GetContent(string)"/></param>
        /// <param name="allowNestedCollections">Specifies whether nested
        /// collections should be included.</param>
        /// <param name="amount">Specifies a limit for the amount of content to parse
        /// from the source. This limit doesn't include the amount of content located
        /// in nested collections.</param>
        public override async Task<RedditListing> ParseSource(string source, bool allowNestedCollections = true, int? amount = null)
        {
            RedditListing content;
            if (amount == null) content = await _source.GetContent(source);
            else content = await _source.GetContent(source, amount.Value);

            //Remove nested collections if we dont want to include them.
            content.Posts = content.GetCollections().Where(i => !i.HasNestedCollection || (i.HasNestedCollection && allowNestedCollections)).ToList();
            //TODO: This filtering should ideally be supported directly by the ISource, since we could then completely avoid populating the contents of the nested collections. (and in that case perform less API calls)

            return content;
        }

        /// <summary>
        /// <see cref="IHandler{T,K}.FetchContent(x,string,x,ICollection{string})"/>
        /// </summary>
        public override async Task FetchContent(RedditListing parsedSource, string targetFolder, RedditFilter filter, ICollection<string> outputLog)
        {
            await Task.Run(async () =>
            {
                if (parsedSource.GetCollections() != null)
                {
                    outputLog.Add($"Starting download of {parsedSource.GetImages().Count()} images.");

                    foreach (var redditPost in parsedSource.GetCollections().Where(image => image != null).AsParallel())
                    {
                        if (redditPost.GetImages() == null) continue;

                        var sync = new object();

                        foreach (var image in redditPost.GetImages().Where(image => image != null).AsParallel())
                        {
                            var imageName = Filenamer.Clean($"{redditPost.ShortTitle} - {await image.GetImageName()}");

                            try
                            {
                                if (filter(await image.GetHeight(), await image.GetWidth(), redditPost.Over_18, redditPost.Album != null))
                                {
                                    var path = Filenamer.DetermineUniqueFilename(Path.Combine(targetFolder, imageName));
                                    var fileContents = await image.GetImage();

                                    File.WriteAllBytes(path, fileContents);

                                    lock (sync)
                                    {
                                        outputLog.Add($"Image saved: {imageName}");
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
                            catch (WebException)
                            {
                                lock (sync)
                                {
                                    outputLog.Add($"Unable to download image: {imageName}");
                                    image.Dispose();
                                }
                            }
                            catch (IOException)
                            {
                                lock (sync)
                                {
                                    outputLog.Add($"IO Failure - Error occured while saving image: {imageName}");
                                    image.Dispose();
                                }
                            }
                        }
                    }
                }

                outputLog.Add("Download finished.");
            });
        }

        public delegate bool RedditFilter(int imageHeight, int imageWidth, bool isNSFW, bool isAlbum);
    }
}
