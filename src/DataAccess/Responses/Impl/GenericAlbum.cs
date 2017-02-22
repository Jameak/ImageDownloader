using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Responses.Impl
{
    public class GenericAlbum : IApiCollection<IApiImage>
    {
        public ICollection<IApiImage> Images { get; set; } = new List<IApiImage>();

        /// <summary>
        /// See <see cref="IApiCollection{T}.GetImages()"/>
        /// </summary>
        public IEnumerable<IApiImage> GetImages()
        {
            return Images;
        }
    }
}
