using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Responses
{
    /// <summary>
    /// Represents image-information returned by an api.
    /// 
    /// Implementations that cache or otherwise locally save information
    /// about the image-file must be disposable.
    /// </summary>
    public interface IApiImage : IApiResponse, IDisposable
    {
        /// <summary>
        /// Gets the byte-array that makes up the entire image-file.
        /// </summary>
        Task<byte[]> GetImage();

        Task<int> GetWidth();

        Task<int> GetHeight();

        /// <summary>
        /// Gets the extension of the image-file.
        /// This extension should include the period.
        /// E.g. .jpg or .png
        /// </summary>
        Task<string> GetImageType();

        /// <summary>
        /// Gets the name of the image.
        /// This name should include the file extension.
        /// </summary>
        Task<string> GetImageName();

        /// <summary>
        /// Gets the aspect ratio of the image, reduced to their lowest rational ratio.
        /// E.g. 1920 by 1080 must be returned as 16 by 9
        /// </summary>
        Task<Tuple<int, int>> GetAspectRatio();
    }
}
