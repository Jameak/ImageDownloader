using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// Interface for access to settings, only really exists to
    /// facilitate dependency injection during testing.
    /// </summary>
    public interface ISettingsManager
    {
        string GetVersionNumber();

        int GetFallbackWidth();
        void SetFallbackWidth(int value);

        int GetFallbackHeight();
        void SetFallbackHeight(int value);

        string GetBuiltinImgurClientID();
        string GetImgurClientID();
        void SetImgurClientID(string value);

        StringCollection GetSupportedExtensions();
        void SetSupportedExtensions(StringCollection value);

        string GetRedditUserAgent();

        string GetRedditAppId();

        string GetDeviantartUserAgent();

        string GetDeviceId();

        string GetGitHubUserAgent();

        void Save();

        void ResetDefaults();
    }
}
