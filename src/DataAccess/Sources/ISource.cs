using System.Threading.Tasks;
using DataAccess.Responses;

namespace DataAccess.Sources
{
    /// <summary>
    /// Provides a mechanism for getting content from an API.
    /// </summary>
    /// <typeparam name="T">The type of content that the API returns.</typeparam>
    public interface ISource<T> where T : IApiResponse
    {
        /// <summary>
        /// Gets the content identified by the given string.
        /// The required content of the string-input varies for each implementation.
        /// </summary>
        /// <returns>
        /// If the content of the returned task implements APIImage, the content of the task may be null.
        /// If the content of the returned task implements APICollection, the content of the task must not be null.
        /// (Reasoning: It makes sense to have null-images, but not null-albums. null-albums are just empty albums)
        /// </returns>
        Task<T> GetContent(string id);
    }

    /// <summary>
    /// Provides a mechanism for getting a specific amount of content from an API
    /// </summary>
    /// <typeparam name="T">The type of content that the API returns.</typeparam>
    public interface ICollectionSource<T> : ISource<T> where T : IApiResponse
    {
        /// <summary>
        /// Gets the content identified by the given string.
        /// The required content of the string-input varies for each implementation.
        /// </summary>
        /// <returns>
        /// If the content of the returned task implements APIImage, the content of the task may be null.
        /// If the content of the returned task implements APICollection, the content of the task must not be null.
        /// (makes sense to have null-images, but not null-albums. null-albums are just empty albums)
        /// </returns>
        Task<T> GetContent(string id, int amount);
    } 
}
