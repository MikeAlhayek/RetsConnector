using Microsoft.Extensions.Logging;
using CrestApps.RetsSdk.Exceptions;
using CrestApps.RetsSdk.Helpers;
using CrestApps.RetsSdk.Models;
using CrestApps.RetsSdk.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace CrestApps.RetsSdk.Services
{
    public class RetsSession : RetsResponseBase<RetsSession>, IRetsSession
    {
        protected readonly IRetsRequester RetsRequester;
        protected readonly ConnectionOptions Options;

        protected Uri LoginUri => new Uri(Options.LoginUrl);
        protected Uri LogoutUri => Resource.GetCapability(Capability.Logout);

        public RetsSession(ILogger<RetsSession> logger, IRetsRequester retsRequester, IOptions<ConnectionOptions> connectionOptions)
            : base(logger)
        {
            RetsRequester = retsRequester;
            Options = connectionOptions.Value;
        }

        private SessionResource _Resource;

        public SessionResource Resource
        {
            get
            {
                return _Resource ?? throw new Exception("Session is not yet started. Please login first.");
            }
        }

        public async Task<bool> Start()
        {

            _Resource = await RetsRequester.Get(LoginUri, async (response) =>
            {
                using (Stream stream = await GetStream(response))
                {
                    XDocument doc = XDocument.Load(stream);

                    AssertValidReplay(doc.Root);

                    XNamespace ns = doc.Root.GetDefaultNamespace();

                    XElement element = doc.Descendants(ns + "RETS-RESPONSE").FirstOrDefault()
                    ?? throw new RetsParsingException("Unable to find the RETS-RESPONSE element in the response.");

                    var parts = element.FirstNode.ToString().Split(Environment.NewLine);
                    var cookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();

                    return GetRetsResource(parts, cookie);
                }
            });

            return IsStarted();

        }

        public async Task End()
        {
            await RetsRequester.Get(LogoutUri, _Resource);

            _Resource = null;
        }


        protected SessionResource GetRetsResource(string[] parts, string cookie)
        {
            var resource = new SessionResource()
            {
                BaseUri = LoginUri.AbsoluteUri.Substring(0, LoginUri.AbsoluteUri.Length - LoginUri.PathAndQuery.Length),
                SessionId = MakeRetsSessionId(cookie),
                Cookie = cookie,
            };

            foreach (var part in parts)
            {
                if (!part.Contains("="))
                {
                    continue;
                }

                var line = part.Split('=');

                if (Enum.TryParse(line[0].Trim(), out Capability result))
                {
                    resource.AddCapability(result, line[1].Trim());
                }
            }

            return resource;
        }

        private string MakeRetsSessionId(string cookie)
        {
            string sessionId = ExtractSessionId(cookie);

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            string agentData = Str.Md5(Options.UserAgent + ":" + Options.UserAgentPassword);

            return $"{agentData}::{sessionId}:{Options.Version.AsHeader()}";
        }

        protected string ExtractSessionId(string cookie)
        {
            var parts = cookie.Split(';');

            foreach (var part in parts)
            {
                string cleanedPart = part.Trim();

                if (!cleanedPart.StartsWith("RETS-Session-ID", StringComparison.CurrentCultureIgnoreCase) || !cleanedPart.Contains("="))
                {
                    continue;
                }

                var line = cleanedPart.Split('=');

                return line[1];
            }

            return null;
        }


        public bool IsStarted()
        {
            return _Resource != null;
        }
    }
}
