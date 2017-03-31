using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Moq;
using Xunit;

namespace Logic.Test
{
    public class UpdateCheckerTest
    {
        private static ISettingsManager CreateSettings(string versionNumber)
        {
            var mock = new Mock<ISettingsManager>();
            mock.Setup(m => m.GetVersionNumber()).Returns(versionNumber);
            return mock.Object;
        }

        private static UpdateChecker SingleReleaseSetup(string currentVersion, string returnVersion)
        {
            var mock = new Mock<ISource<GithubReleases>>();
            var returnVal = new GithubReleases { Releases = new List<GithubRelease> { new GithubRelease { Tag_name = returnVersion } } };
            mock.Setup(m => m.GetContent(null)).ReturnsAsync(returnVal);
            var logic = new UpdateChecker(mock.Object);
            logic.Settings = CreateSettings(currentVersion);

            return logic;
        }
        
        [Fact]
        public async void CheckForUpdate_given_one_release_with_newer_major_returns_update_available()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v6.5.5";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.True(result.Item1);
            Assert.Equal(returnVersion, result.Item2);
        }

        [Fact]
        public async void CheckForUpdate_given_one_release_with_newer_minor_returns_update_available()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v5.6.5";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.True(result.Item1);
            Assert.Equal(returnVersion, result.Item2);
        }

        [Fact]
        public async void CheckForUpdate_given_one_release_with_newer_patch_returns_update_available()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v5.5.6";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.True(result.Item1);
            Assert.Equal(returnVersion, result.Item2);
        }

        [Fact]
        public async void CheckForUpdate_given_one_release_with_older_major_returns_no_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v4.5.5";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.False(result.Item1);
        }

        [Fact]
        public async void CheckForUpdate_given_one_release_with_older_minor_returns_no_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v5.4.5";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.False(result.Item1);
        }

        [Fact]
        public async void CheckForUpdate_given_one_release_with_older_patch_returns_no_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v5.5.4";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.False(result.Item1);
        }
        
        [Fact]
        public async void CheckForUpdate_given_one_release_with_equal_versionnumber_returns_no_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v5.5.5";
            var checker = SingleReleaseSetup(currentVersion, returnVersion);

            var result = await checker.CheckForUpdate();
            Assert.False(result.Item1);
        }
        
        [Fact]
        public async void CheckForUpdate_given_one_draft_release_returns_no_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v6.6.6";

            var mock = new Mock<ISource<GithubReleases>>();
            var returnVal = new GithubReleases { Releases = new List<GithubRelease> { new GithubRelease { Tag_name = returnVersion, Draft = true} } };
            mock.Setup(m => m.GetContent(null)).ReturnsAsync(returnVal);
            var logic = new UpdateChecker(mock.Object);
            logic.Settings = CreateSettings(currentVersion);

            var result = await logic.CheckForUpdate();
            Assert.False(result.Item1);
        }

        [Fact]
        public async void CheckForUpdate_given_one_prerelease_release_returns_no_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersion = "v6.6.6";

            var mock = new Mock<ISource<GithubReleases>>();
            var returnVal = new GithubReleases { Releases = new List<GithubRelease> { new GithubRelease { Tag_name = returnVersion, Prerelease = true } } };
            mock.Setup(m => m.GetContent(null)).ReturnsAsync(returnVal);
            var logic = new UpdateChecker(mock.Object);
            logic.Settings = CreateSettings(currentVersion);

            var result = await logic.CheckForUpdate();
            Assert.False(result.Item1);
        }
        
        [Fact]
        public async void CheckForUpdate_given_no_releases_returns_no_update()
        {
            var currentVersion = "v5.5.5";

            var mock = new Mock<ISource<GithubReleases>>();
            var returnVal = new GithubReleases { Releases = new List<GithubRelease>()};
            mock.Setup(m => m.GetContent(null)).ReturnsAsync(returnVal);
            var logic = new UpdateChecker(mock.Object);
            logic.Settings = CreateSettings(currentVersion);

            var result = await logic.CheckForUpdate();
            Assert.False(result.Item1);
        }

        [Fact]
        public async void CheckForUpdate_given_multiple_releases_returns_new_update()
        {
            var currentVersion = "v5.5.5";
            var returnVersionNew = "v6.6.6";
            var returnVersionOld = "v4.4.4";

            var mock = new Mock<ISource<GithubReleases>>();
            var returnVal = new GithubReleases { Releases = new List<GithubRelease> { new GithubRelease { Tag_name = returnVersionOld }, new GithubRelease { Tag_name = returnVersionNew } } };
            mock.Setup(m => m.GetContent(null)).ReturnsAsync(returnVal);
            var logic = new UpdateChecker(mock.Object);
            logic.Settings = CreateSettings(currentVersion);

            var result = await logic.CheckForUpdate();
            Assert.True(result.Item1);
            Assert.Equal(returnVersionNew, result.Item2);
        }
    }
}
