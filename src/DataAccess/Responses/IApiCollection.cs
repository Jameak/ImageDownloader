using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Responses
{
    /// <summary>
    /// Represents a collection of content returned by an api, such as an album, potentially containing one or more images.
    /// </summary>
    /// <typeparam name="T">The type of IApiImage that the collection contains</typeparam>
    public interface IApiCollection<out T> : IApiResponse where T : class, IApiImage
    {
        /// <summary>
        /// Returns all images contained in the collection. If the collection contains nested collections, the images contained
        /// in these nested collections must be made available through this method as well.
        /// </summary>
        IEnumerable<T> GetImages();
    }

    /// <summary>
    /// Some collections of content returned by an api may potentially contain collections themselves.
    /// If these nested collections should be available, use this interface.
    /// 
    /// An example of a collection containing nested collections is a <see cref="RedditListing"/>.
    /// A RedditListing contains a variable number of <see cref="RedditPost"/>, each of which may link
    /// to an individual image, or a collection of images such as an <see cref="ImgurAlbum"/>.
    /// </summary>
    /// <typeparam name="T">The type of IApiImage that the collection contains</typeparam>
    /// <typeparam name="K">The type of collection contained in this collection</typeparam>
    public interface IApiCollection<out T, out K> : IApiCollection<T> where T : class, IApiImage where K : IApiCollection<T>
    {
        /// <summary>
        /// Provides access to the nested collections in the collection.
        /// </summary>
        IEnumerable<K> GetCollections();
    }
}
