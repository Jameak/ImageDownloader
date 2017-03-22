using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataAccess.Helpers;
using DataAccess.OAuth;
using DataAccess.Responses;
using DataAccess.Responses.Impl;
using Newtonsoft.Json;

namespace DataAccess.Sources
{
    public class RedditSource : ICollectionSource<RedditListing>
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private const int DEFAULT_AMOUNT_OF_CONTENT = 25;
        private const int MAX_POSTS_PER_REQUEST = 100; //API doesn't support more than 100 posts per request.

        //API rules state no more than 60 requests per minute for oauth clients. 
        //Requesting 1000 posts uses 10 requests in potentially less than 1 second
        //(but will generally be much longer since each Reddit-request is interspersed with up to 100 requests to other APIs)
        private const int HARDCAP_CONTENT_AMOUNT = 1000;
        
        private readonly HttpClient _client;
        private readonly ISource<ImgurAlbum> _imgurAlbums;
        private readonly ISource<ImgurImage> _imgurImages;
        private readonly ISource<DeviantartImage> _deviantartImages;
        private readonly ITokenAcquirer<RedditToken> _oauthTokenAcquirer;

        /// <summary>
        /// Initializes a new instance of RedditSource
        /// </summary>
        /// <param name="client">A HttpClient containing a User-Agent header identifying this program</param>
        /// <param name="imgurAlbums">A source of Imgur albums</param>
        /// <param name="imgurImages">A source of Imgur images</param>
        /// <param name="deviantartImages">A source of DeviantArt images</param>
        /// <param name="oauthTokenAcquirer">A source of Reddit OAuth tokens</param>
        public RedditSource(HttpClient client, ISource<ImgurAlbum> imgurAlbums, ISource<ImgurImage> imgurImages, ISource<DeviantartImage> deviantartImages, ITokenAcquirer<RedditToken> oauthTokenAcquirer)
        {
            _client = client;
            _imgurAlbums = imgurAlbums;
            _imgurImages = imgurImages;
            _deviantartImages = deviantartImages;
            _oauthTokenAcquirer = oauthTokenAcquirer;
        }

        /// <summary>
        /// See <see cref="ISource{T}.GetContent(string)"/>
        /// </summary>
        /// <param name="subredditQuery">A identifier specifying the subreddit to pull
        /// content from as well as the query-strings to use during the requests.
        /// 
        /// E.g. "wallpapers/top/.json?sort=top&t=all" will download the top posts of
        /// all time from the wallpapers subreddit.
        /// </param>
        public Task<RedditListing> GetContent(string subredditQuery)
        {
            return GetContent(subredditQuery, DEFAULT_AMOUNT_OF_CONTENT);
        }

        /// <summary>
        /// See <see cref="ICollectionSource{T}.GetContent(string, int)"/>
        /// </summary>
        /// <param name="subredditQuery">A identifier specifying the subreddit to pull
        /// content from as well as the query-strings to use during the requests.
        /// 
        /// E.g. "wallpapers/top/.json?sort=top&t=all" will download the top posts of
        /// all time from the 'wallpapers' subreddit.
        /// </param>
        /// <param name="amount">The amount of content to get. Capped by <see cref="HARDCAP_CONTENT_AMOUNT"/></param>
        public async Task<RedditListing> GetContent(string subredditQuery, int amount)
        {
            if (amount > HARDCAP_CONTENT_AMOUNT) amount = HARDCAP_CONTENT_AMOUNT;

            //Strip end-slash from url to make adding things to the query-string later easier.
            subredditQuery = subredditQuery.Trim('/');

            //Attempt to fix query for callers who forgot to specify .json in the query.
            if (!subredditQuery.Contains(".json"))
            {
                if (subredditQuery.Contains("?"))
                {
                    subredditQuery =
                        $"{subredditQuery.Substring(0, subredditQuery.IndexOf('?'))}.json{subredditQuery.Substring(subredditQuery.IndexOf('?'), subredditQuery.Length)}";
                }
                else
                {
                    subredditQuery += ".json";
                }
            }

            var postsRequested = 0;
            var argAfter = ""; //Holds the value for the after-field in the query string. Lets us page through the content.
            var usefulposts = new BlockingCollection<RedditPost>(); //BlockingCollection to facilitate thread-safe add-operations

            //Since a request can only return a max of 100 posts, we need to page through the content until we've requested enough content.
            while (postsRequested < amount)
            {
                string modifiedQuery = subredditQuery;
                int requestedNow;
                char firstSeperator = subredditQuery.Contains("?") ? '&' : '?';

                //If the amount of content that we need to request is less than the max amount of content that we can request, then only request what we need.
                if (amount - postsRequested <= MAX_POSTS_PER_REQUEST)
                {
                    modifiedQuery += $"{firstSeperator}limit={amount - postsRequested}";
                    requestedNow = amount - postsRequested;
                }
                else
                {
                    modifiedQuery += $"{firstSeperator}limit={MAX_POSTS_PER_REQUEST}";
                    requestedNow = MAX_POSTS_PER_REQUEST;
                }

                if (!string.IsNullOrWhiteSpace(argAfter))
                {
                    modifiedQuery += $"&after={argAfter}&count={postsRequested}";
                }
                
                //We want the oauth token in the header for this request, but not in all requests done by this HttpClient, so add it on a per-request basis.
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://oauth.reddit.com/r/{modifiedQuery}"),
                    Method = HttpMethod.Get
                };
                var token = await _oauthTokenAcquirer.AcquireToken();
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token.Access_token}");
                
                var result = await _client.SendAsync(request);

                if (result.IsSuccessStatusCode)
                {
                    var val = await result.Content.ReadAsStringAsync();
                    var reply = JsonConvert.DeserializeObject<ApiHelper<ApiHelper<ApiHelper<RedditPost>>>>(val);

                    //Ignore selfposts since they dont link to any images.
                    var linkposts = new List<RedditPost>(reply.Data.Children.Where(i => !i.Data.Is_self).Select(i => i.Data));

                    foreach (var post in linkposts.AsParallel())
                    {
                        await ParseResult(post, usefulposts);
                    }
                    
                    postsRequested += requestedNow;
                    argAfter = reply.Data.After;
                }
                else
                {
                    //If the request was unsuccessful, we just abort and return what we've found so far.
                    //We could retry, but if one request failed the rest probably will as well, so better to fail fast.
                    break;
                }
            }

            return new RedditListing { Posts = usefulposts.ToArray() };
        }

        private async Task ParseResult(RedditPost post, BlockingCollection<RedditPost> outputCollection)
        {
            post.Url = post.Url.Trim('/');

            if (post.Domain.Contains(".deviantart.com")) //The domain-field for DeviantArt contains the artists username as well, so we cant switch on it.
            {
                post.Image = await _deviantartImages.GetContent(post.Url);
            }
            else
            {
                switch (post.Domain)
                {
                    case "i.imgur.com":
                    case "m.imgur.com":
                    case "imgur.com":
                        if (post.Url.Contains("imgur.com/a/") || post.Url.Contains("imgur.com/gallery/"))
                        {
                            var albumId = post.Url.Split('/').Last();
                            var album = await _imgurAlbums.GetContent(albumId);
                            post.Album = album;
                        }
                        else
                        {
                            var imageId = post.Url.Split('/').Last();
                            if (imageId.Contains(".")) //The image extension is not a part of the imageId.
                            {
                                imageId = imageId.Split('.').First();
                            }

                            var image = await _imgurImages.GetContent(imageId);
                            post.Image = image;
                        }
                        break;
                    case "i.redd.it": //Images from i.redd.it can be handled just like generic images since it doesn't have an api that we need to call. Case is redundant, but signals that this domain is handled.
                    default:
                        var ending = $".{post.Url.Split('.').Last()}"; //Extensions in the settings-file include the period, so we re-add it after splitting on it.

                        if (Settings.GetSupportedExtensions().Contains(ending.ToLower()))
                        {
                            post.Image = new GenericImage { Url = post.Url };
                        }
                        break;
                }
            }

            //Exclude all posts that link to non-supported domains, or where we couldn't create an image/album-entry.
            if (post.Image != null || post.Album != null)
            {
                outputCollection.Add(post);
            }
        }
    }
}
