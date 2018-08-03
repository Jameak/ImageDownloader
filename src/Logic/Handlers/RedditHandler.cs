using System;
using System.Collections.Concurrent;
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
            //TODO: This filtering should ideally be supported directly by the ISource, since we could then completely avoid populating the contents of the nested collections. (and in that case perform less API calls)
            if (!allowNestedCollections) content.Posts = content.GetCollections().Where(i => !i.IsAlbum).ToList();
            
            return content;
        }

        /// <summary>
        /// <see cref="IHandler{T,K}.FetchContent(x,string,x,ICollection{string})"/>
        /// </summary>
        public override async Task FetchContent(RedditListing parsedSource, string targetFolder, RedditFilter filter, ICollection<string> outputLog, bool saveNestedCollectionsInNestedFolders = false)
        {
            await Task.Run(() =>
            {
                if (parsedSource.GetCollections() != null)
                {
                    outputLog.Add($"Starting download of {parsedSource.GetImages().Count()} images.");
                    

                    foreach (var redditPost in parsedSource.GetCollections().Where(image => image != null))
                    {
                        if (redditPost.GetImages() == null) continue;
                        var sync = new object();
                        var localTargetFolder = targetFolder;

                        //If the images will be saved in their own album, then there is no reason to prefix them with the reddit-title, since the folder has that info already.
                        string imageNamePrefix;

                        if (redditPost.IsAlbum && saveNestedCollectionsInNestedFolders)
                        {
                            localTargetFolder = Path.Combine(localTargetFolder, Filenamer.Clean(redditPost.ShortTitle));
                            imageNamePrefix = string.Empty;
                        }
                        else
                        {
                            imageNamePrefix = Filenamer.Clean(redditPost.ShortTitle) + " - ";
                        }

                        //Limit degree of parallelism to avoid sending too many http-requests at the same time. 
                        //  8 seems like a reasonable amount of requests to have in-flight at a time.
                        var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 8};

                        Parallel.ForEach(redditPost.GetImages().Where(image => image != null), parallelOptions, image =>
                        {
                            using (image)
                            {
                                var imageName = imageNamePrefix + Filenamer.Clean(image.GetImageName().Result);

                                try
                                {
                                    if (filter(image.GetHeight().Result, image.GetWidth().Result, redditPost.Over_18,
                                        redditPost.Album != null, image.GetAspectRatio().Result))
                                    {
                                        //Dont create the folder unless we actually have something to save in it.
                                        if (!Directory.Exists(localTargetFolder)) Directory.CreateDirectory(localTargetFolder);

                                        var path = Filenamer.DetermineUniqueFilename(Path.Combine(localTargetFolder, imageName));
                                        var fileContents = image.GetImage().Result;

                                        File.WriteAllBytes(path, fileContents);
                                        WriteToLog(sync, outputLog, $"Image saved: {imageName}");
                                    }
                                    else
                                    {
                                        WriteToLog(sync, outputLog, $"Image skipped: {imageName}");
                                    }
                                }
                                catch (WebException)
                                {
                                    WriteToLog(sync, outputLog, $"Unable to download image: {imageName}");
                                }
                                catch (IOException)
                                {
                                    WriteToLog(sync, outputLog, $"IO Failure - Error occured while saving image: {imageName}");
                                }
                            }
                        });
                    }
                }

                outputLog.Add("Download finished.");
            });
        }

        public delegate bool RedditFilter(int imageHeight, int imageWidth, bool isNSFW, bool isAlbum, Tuple<int, int> aspectRatio);
    }
}
