using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Responses.Impl;
using DataAccess.Sources;

namespace Logic
{
    public class UpdateChecker
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private readonly ISource<GithubReleases> _githubSource;

        public UpdateChecker(ISource<GithubReleases> githubSource)
        {
            _githubSource = githubSource;
        }

        /// <summary>
        /// Checks whether there is an update available.
        /// </summary>
        /// <returns>
        /// A tuple where:
        /// Item 1 is true if there is an update available, and false otherwise.
        /// Item 2 is the version number of the update, or empty string if no update is available.
        /// </returns>
        public async Task<Tuple<bool, string>> CheckForUpdate()
        {
            var currentVersion = ParseVersion(Settings.GetVersionNumber());
            var releases = await _githubSource.GetContent(null);
            foreach (var githubRelease in releases.Releases)
            {
                if (!githubRelease.Draft && !githubRelease.Prerelease)
                {
                    var releaseVersion = ParseVersion(githubRelease.Tag_name);

                    if (currentVersion.Item1 < releaseVersion.Item1)
                    {
                        return new Tuple<bool, string>(true, githubRelease.Tag_name);
                    }

                    if (currentVersion.Item1 == releaseVersion.Item1 && currentVersion.Item2 < releaseVersion.Item2)
                    {
                        return new Tuple<bool, string>(true, githubRelease.Tag_name);
                    }

                    if (currentVersion.Item1 == releaseVersion.Item1 && currentVersion.Item2 == releaseVersion.Item2 &&
                        currentVersion.Item3 < releaseVersion.Item3)
                    {
                        return new Tuple<bool, string>(true, githubRelease.Tag_name);
                    }
                }
            }

            return new Tuple<bool, string>(false, string.Empty);
        }

        private static Tuple<int, int, int> ParseVersion(string version)
        {
            var cleanVersion = version.StartsWith("v") ? version.Substring(1) : version;
            var labels = cleanVersion.Split('.');

            int major;
            int minor;
            int patch;
            int.TryParse(labels[0], out major);
            int.TryParse(labels[1], out minor);

            //Patch may contain extra non-int info. We dont want to notify about new pre-releases, so failing on those are fine.
            // This will also fail on build-metadata, but I wont be using that so that doesn't matter.
            int.TryParse(labels[2], out patch);

            return new Tuple<int, int, int>(major, minor, patch);
        }
    }
}
