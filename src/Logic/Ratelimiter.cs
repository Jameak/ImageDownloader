using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Helpers;

namespace Logic
{
    /// <summary>
    /// Provides a mechanism for getting information about the current
    /// ratelimits imposed on requests made by the program.
    /// </summary>
    public class Ratelimiter
    {
        private readonly ImgurRatelimiter _imgurLimiter;
        public Ratelimiter(ImgurRatelimiter imgurLimiter)
        {
            _imgurLimiter = imgurLimiter;
        }

        /// <summary>
        /// Loads the limits imposed on the program and returns the limiter values.
        /// </summary>
        /// <returns>
        /// A tuple of ints whose contents are:
        /// Item 1: The max number of client-wide requests.
        /// Item 2: The max number of user-specific requests.
        /// Item 3: The remaining number of client-wide requests.
        /// Item 4: The remaining number of user-specific requests.
        /// </returns>
        public async  Task<Tuple<int,int,int,int>> GetImgurLimitInfo()
        {
            await _imgurLimiter.LoadLimits();
            return _imgurLimiter.GetLimiterValues();
        }
    }
}
