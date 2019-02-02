using RetsSdk.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetsSdk.Services
{
    public class RetsClient
    {
        private ConnectionOptions Options;

        public RetsClient(ConnectionOptions options)
        {
            Options = options;
        }


        public async Task Get(Uri uri, Func<HttpResponseMessage, Task> action)
        {
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Options.UserAgent);
                client.DefaultRequestHeaders.Add("RETS-Version", Options.RetsServerVersion);
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                client.DefaultRequestHeaders.Add("Accept", "*/*");

                var response = await client.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                await action(response);
            }

        }

        public async Task Get(Uri uri, Action<HttpResponseMessage> action, string cookie = null, string sessionId = null, bool ensureSuccessStatusCode = true)
        {

            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Options.UserAgent);
                client.DefaultRequestHeaders.Add("RETS-Version", Options.RetsServerVersion);
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                client.DefaultRequestHeaders.Add("Accept", "*/*");

                if (cookie != null)
                {
                    client.DefaultRequestHeaders.Add("Set-Cookie", cookie);
                }

                if (sessionId != null)
                {
                    client.DefaultRequestHeaders.Add("RETS-Session-ID", sessionId);
                }

                var response = await client.GetAsync(uri);

                if (ensureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                }

                action?.Invoke(response);
            }
        }



        public async Task<T> Get<T>(Uri uri, Func<HttpResponseMessage, Task<T>> action, string cookie = null, string sessionId = null, bool ensureSuccessStatusCode = true) where T : class
        {

            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Options.UserAgent);
                client.DefaultRequestHeaders.Add("RETS-Version", Options.RetsServerVersion);
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                client.DefaultRequestHeaders.Add("Accept", "*/*");

                if (cookie != null)
                {
                    client.DefaultRequestHeaders.Add("Set-Cookie", cookie);
                }

                if (sessionId != null)
                {
                    client.DefaultRequestHeaders.Add("RETS-Session-ID", sessionId);
                }

                var response = await client.GetAsync(uri);

                if (ensureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                }

                return await action?.Invoke(response);
            }
        }

        private HttpClient GetClient()
        {
            if(Options.Type == Models.Enums.AuthenticationType.Digest)
            {
                var credCache = new CredentialCache();
                credCache.Add(new Uri(Options.LoginUrl), Options.Type.ToString(), new NetworkCredential(Options.Username, Options.Password));

                return new HttpClient(new HttpClientHandler { Credentials = credCache });
            }

            // This is wrong and must be implemented for basic authentication
            return new HttpClient();
        }

        public async Task Get(Uri uri, string cookie = null, string sessionId = null, bool ensureSuccessStatusCode = true)
        {
            await Get(uri, null, cookie, sessionId, ensureSuccessStatusCode);
        }
    }
}
