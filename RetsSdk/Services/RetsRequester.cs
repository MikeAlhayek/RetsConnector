using MimeKit;
using MimeTypes.Core;
using RetsSdk.Contracts;
using RetsSdk.Exceptions;
using RetsSdk.Models;
using RetsSdk.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace RetsSdk.Services
{
    public class RetsRequester
    {
        private ConnectionOptions Options;
        private RetsClient Client;

        private SessionResource _Resource;
        protected SessionResource Resource
        {
            get
            {
                return _Resource ?? throw new Exception("Session is not yet started. Please login first.");
            }
        }

        protected Uri LoginUri => new Uri(Options.LoginUrl);
        protected Uri LogoutUri => Resource.GetCapability(Capability.Logout);
        protected Uri GetObjectUri => Resource.GetCapability(Capability.GetObject);
        protected Uri SearchUri => Resource.GetCapability(Capability.Search);
        protected Uri GetMetadataUri => Resource.GetCapability(Capability.GetMetadata);



        public RetsRequester(ConnectionOptions options)
        {
            Options = options ?? throw new NullReferenceException($"{options} cannot be null.");

            Client = new RetsClient(options);
        }

        public async Task LoginRequestAsync()
        {
            _Resource = await Client.Get(LoginUri, async (response) =>
            {
                using (Stream stream = await GetStream(response))
                {
                    XDocument doc = XDocument.Load(stream);

                    AssertSuccessReplayCode(doc.Root);

                    XNamespace ns = doc.Root.GetDefaultNamespace();

                    XElement element = doc.Descendants(ns + "RETS-RESPONSE").FirstOrDefault();

                    if (element == null)
                    {
                        throw new Exception("Unable to find the RETS-RESPONSE element in the response.");
                    }

                    var parts = element.FirstNode.ToString().Split(Environment.NewLine);
                    var cookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();

                    return GetRetsResource(parts, cookie);
                }
            });

        }

        public async Task Search(SearchRequest request)
        {
            if (request == null)
            {
                throw new Exception($"{request} cannot be null");
            }

            var uriBuilder = new UriBuilder(SearchUri);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("SearchType", request.SearchType);
            query.Add("Class", request.Class);
            query.Add("QueryType", request.QueryType);
            query.Add("Count", request.Count.ToString());
            query.Add("Format", request.Format);
            query.Add("Limit", request.Limit.ToString());
            query.Add("StandardNames", request.StandardNames.ToString());
            query.Add("RestrictedIndicator", request.RestrictedIndicator);
            query.Add("Query", request.ParameterGroup.ToString());
            if (request.HasColumns())
            {
                query.Add("Select", string.Join(",", request.GetColumns()));
            }

            uriBuilder.Query = query.ToString();

            await Client.Get(uriBuilder.Uri, async (response) =>
            {
                using (Stream stream = await GetStream(response))
                {
                    XDocument doc = XDocument.Load(stream);

                    AssertSuccessReplayCode(doc.Root);

                    char delimiterValue = GetCompactDelimiter(doc);

                    XNamespace ns = doc.Root.GetDefaultNamespace();
                    XElement columns = doc.Descendants(ns + "COLUMNS").FirstOrDefault();

                    IEnumerable<XElement> records = doc.Descendants(ns + "DATA");

                    var fieldNames = columns.Value.Split(delimiterValue);

                    foreach (var record in records)
                    {
                        var fields = record.Value.Split(delimiterValue);
                    }
                }
            }, Resource.Cookie, Resource.SessionId);

        }

        private char GetCompactDelimiter(XDocument doc)
        {
            XNamespace ns = doc.Root.GetDefaultNamespace();
            XElement delimiter = doc.Descendants(ns + "DELIMITER").FirstOrDefault();

            if (delimiter == null)
            {
                throw new Exception("Unable to find the delimiter! Only 'COMPACT' or 'COMPACT-DECODED' are supported when querying the data");
            }

            var delimiterAttribute = delimiter.Attribute("value");

            if (delimiterAttribute != null && int.TryParse(delimiterAttribute.Value, out int value))
            {
                return Convert.ToChar(value);
            }

            return Convert.ToChar(9);
        }

        public async Task<RetsSystem> GetSystemMetadata(string resourceId = null)
        {
            var uriBuilder = new UriBuilder(GetMetadataUri);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("Type", "METADATA-SYSTEM");
            query.Add("ID", "*");
            query.Add("Format", "STANDARD-XML");

            uriBuilder.Query = query.ToString();

            return await Client.Get(uriBuilder.Uri, async (response) =>
            {
                using (Stream stream = await GetStream(response))
                {
                    XDocument doc = XDocument.Load(stream);

                    AssertSuccessReplayCode(doc.Root);

                    XNamespace ns = doc.Root.GetDefaultNamespace();

                    XElement metaData = doc.Descendants(ns + "METADATA").FirstOrDefault();

                    XElement metadataSystem = metaData.Elements().FirstOrDefault();
                    XElement systemMeta = metadataSystem.Elements().FirstOrDefault();

                    XElement metaDataResource = metadataSystem.Descendants("METADATA-RESOURCE").FirstOrDefault();

                    RetsResourceCollection resources = new RetsResourceCollection();
                    resources.Load(metaDataResource);

                    var system = new RetsSystem()
                    {
                        SystemId = systemMeta.Attribute("SystemID")?.Value,
                        SystemDescription = systemMeta.Attribute("SystemDescription")?.Value,

                        Version = metadataSystem.Attribute("Version")?.Value,
                        Date = DateTime.Parse(metadataSystem.Attribute("Date")?.Value),
                        Resources = resources
                        //system.ForeignKeys
                    };

                    return system;
                }
            }, Resource.Cookie, Resource.SessionId);
        }

        public async Task<RetsResourceCollection> GetResourcesMetadata()
        {
            RetsResourceCollection capsule = await MakeMetadataRequest<RetsResourceCollection>("METADATA-RESOURCE", "0");

            return capsule;
        }

        public async Task<RetsResource> GetResourceMetadata(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException($"{resourceId} cannot be null.");
            }

            RetsResourceCollection capsule = await GetResourcesMetadata();

            var resource = capsule.Get().FirstOrDefault(x => x.ResourceId.Equals(resourceId, StringComparison.CurrentCultureIgnoreCase)) ?? throw new ResourceDoesNotExists();

            return resource;
        }

        public async Task<RetsClassCollection> GetClassesMetadata(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException($"{resourceId} cannot be null.");
            }

            return await MakeMetadataRequest<RetsClassCollection>("METADATA-CLASS", resourceId);
        }

        public async Task<RetsObjectCollection> GetObjectMetadata(string resourceId)
        {
            return await MakeMetadataRequest<RetsObjectCollection>("METADATA-OBJECT", resourceId);
        }


        public async Task<RetsLookupTypeCollection> GetLookupValues(string resourceId, string lookupName)
        {
            var c = await MakeMetadataRequest<RetsLookupCollection>("METADATA-LOOKUP", string.Format("{0}:{1}", resourceId, lookupName));

            return await MakeMetadataRequest<RetsLookupTypeCollection>("METADATA-LOOKUP_TYPE", string.Format("{0}:{1}", resourceId, lookupName));
        }

        public async Task<RetsTableCollection> GetTableMetadata(string resourceId, string lookupName)
        {
            return await MakeMetadataRequest<RetsTableCollection>("METADATA-TABLE", string.Format("{0}:{1}", resourceId, lookupName));
        }


        private async Task<T> ParseMetadata<T>(HttpResponseMessage response)
            where T : class, IRetsCollectionXElementLoader
        {
            using (Stream stream = await GetStream(response))
            {
                XDocument doc = XDocument.Load(stream);

                AssertSuccessReplayCode(doc.Root);

                XNamespace ns = doc.Root.GetDefaultNamespace();

                XElement metaData = doc.Descendants(ns + "METADATA").FirstOrDefault();

                XElement metaDataNode = metaData.Elements().FirstOrDefault();

                T collection = (T)Activator.CreateInstance(typeof(T));
                collection.Load(metaDataNode);

                return collection;
            }
        }

        public async Task<T> MakeMetadataRequest<T>(string type, string id, string format = "STANDARD-XML")
            where T : class, IRetsCollectionXElementLoader
        {
            var uriBuilder = new UriBuilder(GetMetadataUri);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("Type", type);
            query.Add("ID", id);
            query.Add("Format", format);

            uriBuilder.Query = query.ToString();

            return await Client.Get(uriBuilder.Uri, async (response) => await ParseMetadata<T>(response), Resource.Cookie, Resource.SessionId);
        }

        public async Task<IEnumerable<FileObject>> GetObject(string resource, string type, PhotoId id, bool useLocation = false)
        {
            var uriBuilder = new UriBuilder(GetObjectUri);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("Resource", resource);
            query.Add("Type", type);
            query.Add("ID", id.ToString());
            query.Add("Location", useLocation ? "1" : "0");

            uriBuilder.Query = query.ToString();

            return await Client.Get(uriBuilder.Uri, async (response) =>
            {
                var responseContentType = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                var files = new List<FileObject>();

                if (!ContentType.TryParse(responseContentType, out ContentType documentContentType))
                {
                    return files;
                }

                using (Stream memoryStream = await GetStream(response))
                {
                    MimeEntity entity = MimeEntity.Load(documentContentType, memoryStream);

                    if (entity is Multipart multipart)
                    {
                        // At this point we know this is a multi-image response

                        foreach (MimePart part in multipart.OfType<MimePart>())
                        {
                            files.Add(ProcessMessage(part));
                        }

                        return files;
                    }

                    if (entity is MimePart message)
                    {
                        // At this point we know this is a single image response
                        files.Add(ProcessMessage(message));
                    }
                }

                return files;

            }, Resource.Cookie, Resource.SessionId);
        }

        public async Task LogoutRequest()
        {
            await Client.Get(LogoutUri, Resource.Cookie, Resource.SessionId);
        }


        protected async Task<Stream> GetStream(HttpResponseMessage response)
        {
            Stream memoryStream = await response.Content.ReadAsStreamAsync();

            if (response.Content.Headers.ContentEncoding.Any(x => x.Equals("gzip", StringComparison.CurrentCultureIgnoreCase)))
            {
                return new GZipStream(memoryStream, CompressionMode.Decompress);
            }

            return memoryStream;
        }

        protected FileObject ProcessMessage(MimePart message)
        {
            var file = new FileObject()
            {
                ContentId = message.ContentId,
                ObjectId = message.Headers["Object-ID"],
                ContentType = new System.Net.Mime.ContentType(message.ContentType.MimeType),
                ContentDescription = message.Headers["Content-Description"],
                ContentSubDescription = message.Headers["Content-Sub-Description"],
                ContentLocation = message.ContentLocation,
                MemeVersion = message.Headers["MIME-Version"],
                Extension = MimeTypeMap.GetExtension(message.ContentType.MimeType)
            };

            if (bool.TryParse(message.Headers["Preferred"], out bool isPreferred))
            {
                file.IsPreferred = isPreferred;
            }

            if (message.ContentLocation == null)
            {
                file.Content = new MemoryStream();
                message.Content.DecodeTo(file.Content);
            }

            return file;
        }

        protected SessionResource GetRetsResource(string[] parts, string cookie)
        {
            string sessionId = GetSessionId(cookie);
            var resource = new SessionResource()
            {
                SessionId = sessionId,
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

        protected string GetSessionId(string cookie)
        {
            var parts = cookie.Split(';');

            foreach (var part in parts)
            {
                string cleanedPart = part.Trim();

                if (!cleanedPart.StartsWith("RETS-Session-ID") && !cleanedPart.Contains("="))
                {
                    continue;
                }

                var line = cleanedPart.Split('=');

                return line[1];
            }

            return string.Empty;
        }

        protected void AssertSuccessReplayCode(XElement root)
        {
            var replayCode = root.Attribute("ReplyCode");

            if (replayCode == null)
            {
                throw new MissingFieldException("Unable to find ReplyCode attribute on the XElement.");
            }

            if (int.TryParse(replayCode.Value, out int code))
            {
                if (code == 0)
                {
                    // At this point we know the request is successfull
                    return;
                }

                var replayText = root.Attribute("ReplyText");

                throw new Exception(replayText?.Value ?? "Unknown error");
            }
        }
    }
}
