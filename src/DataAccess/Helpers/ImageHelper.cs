using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// The amount of time to wait for a download to finish before cancelling it.
        /// </summary>
        public const int DOWNLOAD_TIMEOUT_SECONDS = 60;

        /// <summary>
        /// Downloads the content located at the given url.
        /// Throws WebException if the request fails or takes too long.
        /// </summary>
        public static async Task<byte[]> GetWebContent(string url)
        {
            if (url == null) return null;
            var tokensource = new CancellationTokenSource();
            tokensource.CancelAfter(TimeSpan.FromSeconds(DOWNLOAD_TIMEOUT_SECONDS));

            try
            {
                var request = WebRequest.CreateHttp(url);
                tokensource.Token.Register(request.Abort);
                using (var stream = (await request.GetResponseAsync()).GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms, 81920, tokensource.Token);
                    return ms.ToArray();
                }
            }
            catch (TaskCanceledException e)
            {
                throw new WebException("Connection lost", e);
            }
        }

        /// <summary>
        /// Non-recursive GCD
        /// </summary>
        public static int GCD(int a, int b)
        {
            while (true)
            {
                if (b == 0) return a;
                var a1 = a;
                a = b;
                b = a1 % b;
            }
        }

        /// <summary>
        /// Reduces the given ratio to their lowest rational representation.
        /// </summary>
        public static Tuple<int, int> GetAspectRatio(int width, int height)
        {
            var den = GCD(width, height);
            var item1 = width / den;
            var item2 = height / den;
            return new Tuple<int, int>(item1, item2);
        }
    }
}
