using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WinBoxAgent
{
    public class OAuthMessageHandler : DelegatingHandler
    {
        public OAuthMessageHandler(HttpMessageHandler handler)
            : base(handler)
        {
            _authBase = new OAuthBase();
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string normalizedUri;
            string authHeader;
            string normalizedParameters;

            _authBase.GenerateSignature(
                request.RequestUri,
                AppKey,
                AppSecret,
                UserToken,
               UserSecret,
                request.Method.Method,
                _authBase.GenerateTimeStamp(),
                _authBase.GenerateNonce(),
                out normalizedUri,
                out normalizedParameters,
                out authHeader);

            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader);

            return base.SendAsync(request, cancellationToken);
        }

        public string GetHeader(string url)
        {
            string normalizedUri;
            string authHeader;
            string normalizedParameters;

            _authBase.GenerateSignature(
                new Uri(url),
                AppKey,
                AppSecret,
                UserToken,
               UserSecret,
               "PUT",
                _authBase.GenerateTimeStamp(),
                _authBase.GenerateNonce(),
                out normalizedUri,
                out normalizedParameters,
                out authHeader);

            return authHeader;
        }

        readonly OAuthBase _authBase;
        private const string UserSecret = "h18w80z4c7h5gmn";
        private const string UserToken = "dkms8xc4x3mtm19";

        private const string AppKey = "7zqu95o832tr0kb";
        private const string AppSecret = "j1k6gklasd5knv5";
    }
}