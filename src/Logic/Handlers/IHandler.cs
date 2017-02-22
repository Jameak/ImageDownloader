using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Responses;
using DataAccess.Sources;

namespace Logic.Handlers
{
    /// <summary>
    /// Provides a mechanism for performing API-calls to various sources
    /// (<see cref="ISource{T}"/>) and saving the images contained in the
    /// reply from those sources to disk.
    /// </summary>
    /// <typeparam name="T">A delegate whose signature specifies what
    /// values should be checked to determine whether to include or exclude
    /// the image.</typeparam>
    /// <typeparam name="K">The type of collection that the parsing creates.</typeparam>
    public interface IHandler<T, K> where T : class where K : IApiCollection<IApiImage>
    {
        /// <summary>
        /// Provides a mechanism for parsing the content located at the given
        /// source into a representation that can be filtered and downloaded later.
        /// </summary>
        /// <param name="source">The source of information. 
        /// The format of this string varies by implementation.</param>
        /// <param name="allowNestedCollections"> The collection returned
        /// by some api-sources may contain inner collections.
        /// This specifies whether these nested collections should be
        /// included. Not all implementations support disallowing the
        /// nested collections.</param>
        /// <param name="amount">Optional parameter specifying how much
        /// content to parse. Not all implementations support specifying
        /// the amount of content to parse.</param>
        /// <returns>A container with metadata about the content located
        /// at the source.</returns>
        Task<K> ParseSource(string source, bool allowNestedCollections = true, int? amount = null);
         
        /// <summary>
        /// Provides a mechanism for downloading content to the specified
        /// folder based on the parsed source, if the content satisfies
        /// the specified filter.
        /// </summary>
        /// <param name="parsedSource">A parsed representaton of the content
        /// that should be downloaded.</param>
        /// <param name="targetFolder">The location that the content
        /// should be saved.</param>
        /// <param name="filter">A filter specifying the requirements that
        /// each image must satisfy to be included in the download.</param>
        /// <param name="outputLog">During download of the content, information
        /// about the download-progress will be added to this collection</param>
        Task FetchContent(K parsedSource, string targetFolder, T filter, ICollection<string> outputLog);
    }
}
