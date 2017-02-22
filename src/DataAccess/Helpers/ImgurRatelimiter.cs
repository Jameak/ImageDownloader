using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Exceptions;
using DataAccess.Responses.Impl;
using DataAccess.Sources;

namespace DataAccess.Helpers
{
    /// <summary>
    /// Provides a mechanism for determining whether requests to the Imgur
    /// API should be allowed.
    /// </summary>
    public class ImgurRatelimiter
    {
        private readonly ISource<ImgurRatelimitResponse> _limitSource;
        private readonly object _sync = new object();

        /// <summary>
        /// This limit resets daily.
        /// </summary>
        private int _remainingClientRequests;
        
        /// <summary>
        /// This limit resets hourly.
        /// </summary>
        private int _remainingUserRequests;

        private int _clientLimit;
        private int _userLimit;
        private bool _limitsLoaded;

        public ImgurRatelimiter(ISource<ImgurRatelimitResponse> limitSource)
        {
            _limitSource = limitSource;
        }

        /// <summary>
        /// Attempts to load the limit-values for the programs Client-ID.
        /// 
        /// When loading ratelimits, callers can choose to silently ignore loadfailure.
        /// Loadfailure will leave the ratelimiter in such a state that no requests will be allowed.
        /// 
        /// Failure to load is typically caused by an invalid client-id.
        /// </summary>
        internal virtual async Task AttemptToLoadLimits()
        {
            try
            {
                await LoadLimits();
            }
            catch (InvalidClientIDException)
            {
                return;
            }
        }

        /// <summary>
        /// Attempts to load the limit-values for the programs Client-ID.
        /// Loadfailure will leave the ratelimiter in such a state that no requests will be allowed.
        /// 
        /// Failure to load is typically caused by an invalid client-id.
        /// </summary>
        public virtual async Task LoadLimits()
        {
            var result = await _limitSource.GetContent(null);

            if (result == null) //Most likely caused by an internet problem
            {
                _remainingClientRequests = -1;
                _remainingUserRequests = -1;
                _clientLimit = -1;
                _userLimit = -1;
                _limitsLoaded = false;
                return;
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                _remainingClientRequests = -1;
                _remainingUserRequests = -1;
                _clientLimit = -1;
                _userLimit = -1;
                _limitsLoaded = false;
                throw new InvalidClientIDException("Provided Client-ID is invalid.");
            }

            lock (_sync)
            {
                _remainingClientRequests = result.ClientRemaining;
                _remainingUserRequests = result.UserRemaining;
                _clientLimit = result.ClientLimit;
                _userLimit = result.UserLimit;
                _limitsLoaded = true;
            }
        }

        public virtual bool IsRequestAllowed()
        {
            lock (_sync)
            {
                var val = _remainingUserRequests > 0 && _remainingClientRequests >= 100; //Keep a small buffer for the shared client-wide limit.

                if (val)
                {
                    //Since IsRequestAllowed signals an intent to hit the API, we can decrement early. 
                    _remainingUserRequests--;
                }
                return val;
            }
        }

        /// <summary>
        /// Updates the internal values of the ratelimiter to the ones included in the header,
        /// in such a way that multithreaded usage of the ratelimiter-object will never cause the limiter-values
        /// to increase due to out-of-order limit-updates.
        /// 
        /// Will automatically detect the rate-limits being reset.
        /// </summary>
        public virtual void UpdateLimit(ICollection<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            lock (_sync)
            {
                foreach (var header in headers)
                {
                    if (header.Key == "X-RateLimit-ClientRemaining")
                    {
                        var value = int.Parse(header.Value.First());

                        //If we have a lot of requests left, being accurate isn't important, so this is an easy way to catch the client-wide limit being reset.
                        if (value < _remainingClientRequests || value >= _clientLimit - 200)
                        {
                            _remainingClientRequests = value;
                        }
                    } else if (header.Key == "X-RateLimit-UserLimit")
                    {
                        var value = int.Parse(header.Value.First());

                        //If we have a lot of requests left, being accurate isn't important, so this is an easy way to catch the user-limit being reset.
                        if (value < _remainingUserRequests || value >= _userLimit - 50)
                        {
                            _remainingUserRequests = value;
                        }
                    }
                }

                //IEnumerable<string> clientRemaining;
                //headers.TryGetValues("X-RateLimit-ClientRemaining", out clientRemaining);
                //if (clientRemaining != null)
                //{
                //    var value = int.Parse(clientRemaining.First());

                //    //If we have a lot of requests left, being accurate isn't important, so this is an easy way to catch the client-wide limit being reset.
                //    if (value < _remainingClientRequests || value >= _clientLimit - 200)
                //    {
                //        _remainingClientRequests = value;
                //    }
                //}

                //IEnumerable<string> userRemaining;
                //headers.TryGetValues("X-RateLimit-UserLimit", out userRemaining);
                //if (clientRemaining != null)
                //{
                //    var value = int.Parse(userRemaining.First());

                //    //If we have a lot of requests left, being accurate isn't important, so this is an easy way to catch the user-limit being reset.
                //    if (value < _remainingUserRequests || value >= _userLimit - 50)
                //    {
                //        _remainingUserRequests = value;
                //    }
                //}
            }
        }

        public virtual bool LimitsHaveBeenLoaded()
        {
            lock (_sync)
            {
                return _limitsLoaded;
            }
        }

        /// <summary>
        /// Returns the limiter values.
        /// </summary>
        /// <returns>
        /// A tuple of ints whose contents are:
        /// Item 1: The max number of client-wide requests.
        /// Item 2: The max number of user-specific requests.
        /// Item 3: The remaining number of client-wide requests.
        /// Item 4: The remaining number of user-specific requests.
        /// </returns>
        public virtual Tuple<int,int,int,int> GetLimiterValues()
        {
            lock (_sync)
            {
                return new Tuple<int, int, int, int>(_clientLimit, _userLimit, _remainingClientRequests, _remainingUserRequests);
            }
        }
    }
}
