using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Responses;

namespace DataAccess.OAuth
{
    public class RedditToken : IApiResponse
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public string Token_type { get; set; }
        public string Scope { get; set; }

        public int AcquisitionTime { get; set; }
    }
}
