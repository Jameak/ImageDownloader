using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Responses.Impl
{
    public class RedditListing : IApiCollection<IApiImage, RedditPost>
    {
        public ICollection<RedditPost> Posts { private get; set; } = new List<RedditPost>();

        /// <summary>
        /// See <see cref="IApiCollection{T,K}.GetCollections()"/>
        /// </summary>
        public IEnumerable<RedditPost> GetCollections()
        {
            return Posts;
        }

        /// <summary>
        /// See <see cref="IApiCollection{T}.GetImages()"/>
        /// </summary>
        public IEnumerable<IApiImage> GetImages()
        {
            var list = new List<IApiImage>();

            foreach (var post in Posts.Where(post => post.GetImages() != null))
            {
                list.AddRange(post.GetImages());
            }

            return list;
        }
    }
}
