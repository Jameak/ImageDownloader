using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Responses.Impl
{
    public class LocalDirectory : IApiCollection<LocalImage>
    {
        public ICollection<LocalImage> Images { get; set; } = new List<LocalImage>();

        /// <summary>
        /// See <see cref="IApiCollection{T}.GetImages()"/>
        /// </summary>
        public IEnumerable<LocalImage> GetImages()
        {
            return Images;
        }
    }
}
