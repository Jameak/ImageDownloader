using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Responses.Impl
{
    public class GithubRelease : IApiResponse
    {
        public string Tag_name { get; set; }
        public bool Draft { get; set; }
        public bool Prerelease { get; set; }
    }

    public class GithubReleases : IApiResponse
    {
        public ICollection<GithubRelease> Releases { get; set; } = new List<GithubRelease>();
    }
}
