using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Test
{
    public class StubHttpClient
    {
        public static HttpClient Create(FakeResponseHandler handler)
        {
            return new HttpClient(handler);
        }

        public static FakeResponseHandler GetHandler()
        {
            return new FakeResponseHandler();
        }
        
        public class FakeResponseHandler : DelegatingHandler
        {
            private readonly Dictionary<Uri, HttpResponseMessage> _fakeResponses = new Dictionary<Uri, HttpResponseMessage>();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return _fakeResponses.ContainsKey(request.RequestUri)
                    ? _fakeResponses[request.RequestUri]
                    : new HttpResponseMessage(HttpStatusCode.NotFound) {RequestMessage = request};
            }

            public void AddResponse<T>(Uri uri, HttpStatusCode status, T content)
            {
                var response = new HttpResponseMessage(status)
                {
                    Content = new ObjectContent<T>(content, new JsonMediaTypeFormatter(), "application/json")
                };

                _fakeResponses.Add(uri, response);
            }
        }
    }
}
