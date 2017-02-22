using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses;

namespace DataAccess.OAuth
{
    public interface ITokenAcquirer<T> where T : IApiResponse
    {
        /// <summary>
        /// Used to acquire an oauth token.
        /// 
        /// The token must be valid at acquisition time,
        /// but a cached token may be returned if it hasn't expired yet.
        /// </summary>
        Task<T> AcquireToken();
    }
}
