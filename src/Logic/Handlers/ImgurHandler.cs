using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DataAccess.Responses;
using DataAccess.Responses.Impl;
using DataAccess.Sources;

namespace Logic.Handlers
{
    public class ImgurHandler : AbstractHandler<ImgurHandler.ImgurFilter, IApiCollection<IApiImage>>
    {
        private readonly ISource<ImgurAlbum> _albumSource;
        private readonly ISource<GenericAlbum> _accountContentSource;

        public ImgurHandler(ISource<ImgurAlbum> albumSource, ISource<GenericAlbum> accountContentSource)
        {
            _albumSource = albumSource;
            _accountContentSource = accountContentSource;
        }

        /// <summary>
        /// <see cref="IHandler{T,K}.ParseSource(string,bool,Nullable{int})"/>
        /// </summary>
        /// <param name="source">A url specifying the source to parse.
        /// This source must be a url identifying an Imgur album or
        /// an Imgur account page.</param>
        /// <param name="allowNestedCollections">Not supported parameter</param>
        /// <param name="amount">Not supported parameter</param>
        public override async Task<IApiCollection<IApiImage>> ParseSource(string source, bool allowNestedCollections = true, int? amount = null)
        {
            IApiCollection<IApiImage> content;

            try
            {
                switch (ContentType(source))
                {
                    case Content.Album:
                        content = await _albumSource.GetContent(source.Split('/').Last()); //Albums are imgur.com/a/{id} or imgur.com/gallery/{id}
                        break;
                    case Content.AccountImages:
                        content = await _accountContentSource.GetContent(source.Split('.').First().Split('/').Last()); //Accounts are {username}.imgur.com (but just {username} input is allowed)
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }

            return content;
        }

        /// <summary>
        /// <see cref="IHandler{T,K}.FetchContent(x,string,x,ICollection{string})"/>
        /// </summary>
        /// <param name="saveNestedCollectionsInNestedFolders">Imgur collections
        /// cannot contain nested albums, so the parameter has no effect.</param>
        public override async Task FetchContent(IApiCollection<IApiImage> parsedSource, string targetFolder, ImgurFilter filter, ICollection<string> outputLog, bool saveNestedCollectionsInNestedFolders = false)
        {
            await Task.Run(() =>
            {
                if (parsedSource.GetImages() != null)
                {
                    outputLog.Add($"Starting download of {parsedSource.GetImages().Count()} images.");

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
                            catch (AggregateException ex)
                            {
                                if (ex.InnerException is WebException)
                                {
                                    WriteToLog(sync, outputLog, $"Unable to download image: {imageName}");
                                }
                                else
                                {
                                    WriteToLog(sync, outputLog, $"Unknown error occured for {imageName}. Error message is: " + ex.InnerException.Message);
                                }
                            }
                        }
                    });
                }

                outputLog.Add("Download finished.");
            });
        }

        private static Content ContentType(string content)
        {
            if (content.Contains("imgur.com/a/") || content.Contains("imgur.com/gallery/"))
            {
                return Content.Album;
            }
            return Content.AccountImages;
        }

        private enum Content
        {
            Album, AccountImages
        }

        public delegate bool ImgurFilter(int imageHeight, int imageWidth, Tuple<int, int> aspectRatio);
    }
}
