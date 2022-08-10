using Microsoft.Extensions.Logging;
using CrestApps.RetsSdk.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CrestApps.RetsSdk.Services
{
    public class RetsWebRequester : IRetsRequester
    {
        private readonly ConnectionOptions Options;
        private readonly IHttpClientFactory HttpClientFactory;

        public RetsWebRequester(IOptions<ConnectionOptions> options, IHttpClientFactory httpClientFactory)
        {
            Options = options.Value;
            HttpClientFactory = httpClientFactory;
        }


        public async Task Get(Uri uri, Action<HttpResponseMessage> action, SessionResource resource = null, bool ensureSuccessStatusCode = true)
        {
            using (var client = GetClient(resource))
            {
                var response = await client.GetAsync(uri);

                if (ensureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                }

                action?.Invoke(response);
            }
        }

        public async Task<T> Get<T>(Uri uri, Func<HttpResponseMessage, Task<T>> action, SessionResource resource = null, bool ensureSuccessStatusCode = true) where T : class
        {
            using (var client = GetClient(resource))
            {
                var response = await client.GetAsync(uri);

                if (ensureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                }

                return await action?.Invoke(response);
            }
        }


        public async Task Get(Uri uri, SessionResource resource = null, bool ensureSuccessStatusCode = true)
        {
            await Get(uri, null, resource, ensureSuccessStatusCode);
        }

        protected virtual HttpClient GetClient(SessionResource resource)
        {
            HttpClient client = GetAuthenticatedClient();

            client.Timeout = Options.Timeout;
            client.DefaultRequestHeaders.Add("User-Agent", Options.UserAgent);
            client.DefaultRequestHeaders.Add("RETS-Version", Options.Version.AsHeader());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.Add("Accept", "*/*");

            if (resource != null && !string.IsNullOrWhiteSpace(resource.Cookie))
            {
                //client.DefaultRequestHeaders.Add("Set-Cookie", resource.Cookie);
                client.DefaultRequestHeaders.Add("Cookie", resource.Cookie);
            }

            if (resource != null && !string.IsNullOrWhiteSpace(resource.SessionId))
            {
                client.DefaultRequestHeaders.Add("RETS-Session-ID", resource.SessionId);
            }

            return client;
        }

        private HttpClient GetAuthenticatedClient()
        {
            if (Options.Type == Models.Enums.AuthenticationType.Digest)
            {
                var credCache = new CredentialCache();
                credCache.Add(new Uri(Options.LoginUrl), Options.Type.ToString(), new NetworkCredential(Options.Username, Options.Password));

                return new HttpClient(new HttpClientHandler { Credentials = credCache });
            }

            HttpClient client = HttpClientFactory.CreateClient();

            byte[] byteArray = Encoding.ASCII.GetBytes($"{Options.Username}:{Options.Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }
    }
}
