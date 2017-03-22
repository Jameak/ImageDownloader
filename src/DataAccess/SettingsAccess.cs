using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// Wraps the internal settings-file to provide access to settings,
    /// and automatically generates a deviceId for each install.
    /// 
    /// Also does locking, but the settings-file _should_ be thread-safe already,
    /// so that part probably doesn't matter...
    /// </summary>
    public class SettingsAccess : ISettingsManager
    {
        private static readonly object _sync = new object();
        private static SettingsAccess Instance;

        private SettingsAccess() { }

        public static SettingsAccess GetInstance()
        {
            lock (_sync)
            {
                return Instance ?? (Instance = new SettingsAccess());
            }
        }

        public string GetVersionNumber()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.VersionNumber;
            }
        }

        public int GetFallbackWidth()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.FallbackDimensionWidth;
            }
        }

        public void SetFallbackWidth(int value)
        {
            lock (_sync)
            {
                Properties.Settings.Default.FallbackDimensionWidth = value;
            }
        }

        public int GetFallbackHeight()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.FallbackDimensionHeight;
            }
        }

        public void SetFallbackHeight(int value)
        {
            lock (_sync)
            {
                Properties.Settings.Default.FallbackDimensionHeight = value;
            }
        }

        public string GetBuiltinImgurClientID()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.ImgurClientIDDefault;
            }
        }

        public string GetImgurClientID()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.ImgurClientID;
            }
        }

        public void SetImgurClientID(string value)
        {
            lock (_sync)
            {
                Properties.Settings.Default.ImgurClientID = value;
            }
        }

        public StringCollection GetSupportedExtensions()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.SupportedExtensions;
            }
        }

        public void SetSupportedExtensions(StringCollection value)
        {
            lock (_sync)
            {
                Properties.Settings.Default.SupportedExtensions = value;
            }
        }

        public string GetRedditUserAgent()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.RedditUserAgent;
            }
        }

        public string GetRedditAppId()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.RedditAppId;
            }
        }

        public string GetDeviantartUserAgent()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.DeviantartUserAgent;
            }
        }

        public string GetGitHubUserAgent()
        {
            lock (_sync)
            {
                return Properties.Settings.Default.GitHubUserAgent;
            }
        }

        public string GetDeviceId()
        {
            lock (_sync)
            {
                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DeviceId))
                {
                    //Limit the length of the GUID since the reddit api wants a 20-30 length device id.
                    Properties.Settings.Default.DeviceId = Guid.NewGuid().ToString("N").Substring(0, 25);
                    Properties.Settings.Default.Save();
                }

                return Properties.Settings.Default.DeviceId;
            }
        }

        public void Save()
        {
            lock (_sync)
            {
                Properties.Settings.Default.Save();
            }
        }

        public void ResetDefaults()
        {
            lock (_sync)
            {
                Properties.Settings.Default.Reset();
            }
        }
    }
}