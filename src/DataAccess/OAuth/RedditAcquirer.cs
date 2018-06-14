using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataAccess.OAuth
{
    /// <summary>
    /// Acquires a valid OAuth token for Reddit.
    /// </summary>
    public class RedditAcquirer : ITokenAcquirer<RedditToken>
    {
        private ISettingsManager _settings = SettingsAccess.GetInstance();
        public ISettingsManager Settings
        {
            private get { return _settings; }
            set { _settings = value; SetClientInfo(); }
        }

        private HttpClient _client;
        private RedditToken _token;

        public RedditAcquirer()
        {
            SetClientInfo();
        }

        private void SetClientInfo()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Settings.GetDeviantartUserAgent());

            //Basic HTTP auth header is username:password encoded in base64.
            var basicAuth = $"{Settings.GetRedditAppId()}:";
            var basicBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicAuth));
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {basicBase64}");
        }

        /// <inheritdoc />
        public async Task<RedditToken> AcquireToken()
        {
            if (_token != null)
            {
                //If the token we've already grabbed is still valid, just return that one.
                //Invalidate tokens 5 min before they expire incase the token is used for multiple requests over several minutes.
                if (!(_token.AcquisitionTime + _token.Expires_in < CurrentTimeSeconds() - 300))
                {
                    return _token;
                }
            }

            var content =
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "https://oauth.reddit.com/grants/installed_client"),
                    new KeyValuePair<string, string>("device_id", Settings.GetDeviceId())
                });

            var result = await _client.PostAsync("https://www.reddit.com/api/v1/access_token", content);

            if (result.IsSuccessStatusCode)
            {
                var val = await result.Content.ReadAsStringAsync();
                _token = JsonConvert.DeserializeObject<RedditToken>(val);

                _token.AcquisitionTime = CurrentTimeSeconds();
                return _token;
            }

            return null;
        }

        private static int CurrentTimeSeconds()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
