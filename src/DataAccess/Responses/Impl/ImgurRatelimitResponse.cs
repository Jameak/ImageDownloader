using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Responses.Impl
{
    public class ImgurRatelimitResponse : IApiResponse
    {
        public int UserLimit { get; set; }
        public int UserRemaining { get; set; }
        public int UserReset { get; set; }
        public int ClientLimit { get; set; }
        public int ClientRemaining { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
