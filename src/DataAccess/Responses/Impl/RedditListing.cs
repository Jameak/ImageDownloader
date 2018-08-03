using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Responses.Impl
{
    public class RedditListing : IApiCollection<IApiImage, RedditPost>
    {
        public ICollection<RedditPost> Posts { private get; set; } = new List<RedditPost>();

        /// <inheritdoc />
        public IEnumerable<RedditPost> GetCollections()
        {
            return Posts;
        }

        /// <inheritdoc />
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
