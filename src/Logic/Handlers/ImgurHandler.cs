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
        public override async Task FetchContent(IApiCollection<IApiImage> parsedSource, string targetFolder, ImgurFilter filter, ICollection<string> outputLog)
        {
            await Task.Run(async () =>
            {
                if (parsedSource.GetImages() != null)
                {
                    outputLog.Add($"Starting download of {parsedSource.GetImages().Count()} images.");

                    var sync = new object();

                    foreach (var image in parsedSource.GetImages().Where(image => image != null).AsParallel())
                    {
                        var imageName = Filenamer.Clean(await image.GetImageName());

                        try
                        {
                            if (filter(await image.GetHeight(), await image.GetWidth()))
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

        public delegate bool ImgurFilter(int imageHeight, int imageWidth);
    }
}
