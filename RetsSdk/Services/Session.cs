using Microsoft.Extensions.Logging;
using MimeKit;
using MimeTypes.Core;
using RetsSdk.Contracts;
using RetsSdk.Exceptions;
using RetsSdk.Helpers.Extensions;
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
    public class Session
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
        protected ILogger Log { get; private set; }

        public Session(ConnectionOptions options)
        {
            Options = options ?? throw new NullReferenceException($"{options} cannot be null.");

            Client = new RetsClient(options);
        }

        public Session(ConnectionOptions options, ILogger log)
            : this(options)
        {
            SetLogger(log);
        }

        public void SetLogger(ILogger log)
        {
            Log = log;
        }

        public async Task Connect()
        {
            try
            {
                // clear up any outstanding connections
                await Disconnect();
            }
            catch { }

            _Resource = await Client.Get(LoginUri, async (response) =>
            {
                using (Stream stream = await GetStream(response))
                {
                    XDocument doc = XDocument.Load(stream);

                    AssertValidReplay(doc.Root);

                    XNamespace ns = doc.Root.GetDefaultNamespace();

                    XElement element = doc.Descendants(ns + "RETS-RESPONSE").FirstOrDefault();

                    if (element == null)
                    {
                        throw new RetsParsingException("Unable to find the RETS-RESPONSE element in the response.");
                    }

                    var parts = element.FirstNode.ToString().Split(Environment.NewLine);
                    var cookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();

                    return GetRetsResource(parts, cookie);
                }
            });

        }

        public async Task<SearchResult> Search(SearchRequest request)
        {
            if (request == null)
            {
                throw new Exception($"{request} cannot be null");
            }

            RetsResource resource = await GetResourceMetadata(request.SearchType);

            if (resource == null)
            {
                string message = string.Format("The provided '{0}' is not valid. You can get a list of all valid value by calling '{1}' method on the Session object.", nameof(SearchRequest.SearchType), nameof(GetResourcesMetadata));

                throw new Exception(message);
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
                var columns = request.GetColumns().ToList();

                if (!request.HasColumn(resource.KeyField))
                {
                    columns.Add(resource.KeyField);
                }

                query.Add("Select", string.Join(",", columns));
            }

            uriBuilder.Query = query.ToString();

            return await Client.Get(uriBuilder.Uri, async (response) =>
            {
                using (Stream stream = await GetStream(response))
                {
                    XDocument doc = XDocument.Load(stream);

                    int code = GetReplayCode(doc.Root);

                    AssertValidReplay(doc.Root, code);

                    var result = new SearchResult(resource, request.Class, request.RestrictedIndicator);

                    if (code == 0)
                    {
                        char delimiterValue = GetCompactDelimiter(doc);

                        XNamespace ns = doc.Root.GetDefaultNamespace();
                        XElement columns = doc.Descendants(ns + "COLUMNS").FirstOrDefault();

                        IEnumerable<XElement> records = doc.Descendants(ns + "DATA");



                        string[] tableColumns = columns.Value.Split(delimiterValue);
                        result.SetColumns(tableColumns);

                        foreach (var record in records)
                        {
                            string[] fields = record.Value.Split(delimiterValue);

                            SearchResultRow row = new SearchResultRow(tableColumns, fields, resource.KeyField, request.RestrictedIndicator);

                            result.AddRow(row);
                        }
                    }

                    return result;
                }
            }, Resource.Cookie, Resource.SessionId);
        }

        public async Task<RetsSystem> GetSystemMetadata()
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

                    AssertValidReplay(doc.Root);

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
            //var c = await MakeMetadataRequest<RetsLookupCollection>("METADATA-LOOKUP", string.Format("{0}:{1}", resourceId, lookupName));

            return await MakeMetadataRequest<RetsLookupTypeCollection>("METADATA-LOOKUP_TYPE", string.Format("{0}:{1}", resourceId, lookupName));
        }

        public async Task<RetsFieldCollection> GetTableMetadata(string resourceId, string className)
        {
            return await MakeMetadataRequest<RetsFieldCollection>("METADATA-TABLE", string.Format("{0}:{1}", resourceId, className));
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
            return await GetObject(resource, type, new List<PhotoId> { id }, useLocation);
        }


        public async Task<IEnumerable<FileObject>> GetObject(string resource, string type, IEnumerable<PhotoId> ids, int batchSize, bool useLocation = false)
        {
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentNullException($"{nameof(resource)} cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException($"{nameof(type)} cannot be null.");
            }

            if (ids == null)
            {
                throw new ArgumentNullException($"{nameof(ids)} cannot be null.");
            }

            List<FileObject> files = new List<FileObject>();

            IEnumerable<IEnumerable<PhotoId>> pages = ids.Partition(batchSize);

            foreach (var page in pages)
            {
                // To prevent having to many outstanding requests
                // we should connect, force round trip on every page 

                IEnumerable<FileObject> _files = await RoundTrip(async () =>
                {
                    return await GetObject(resource, type, page, useLocation);
                });

                files.AddRange(_files);
            }

            return files;
        }



        public async Task RoundTrip(Func<Task> action)
        {
            try
            {
                if (!IsConnected())
                {
                    await Connect();
                }

                action?.Invoke();

            }
            catch
            {
                throw;
            }
            finally
            {
                await Disconnect();
            }
        }

        public bool IsConnected()
        {
            return _Resource != null;
        }


        public async Task<TResult> RoundTrip<TResult>(Func<Task<TResult>> action)
        {
            try
            {
                if (!IsConnected())
                {
                    await Connect();
                }

                TResult result = await action.Invoke();
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                await Disconnect();
            }
        }



        public async Task<IEnumerable<FileObject>> GetObject(string resource, string type, IEnumerable<PhotoId> ids, bool useLocation = false)
        {
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentNullException($"{nameof(resource)} cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException($"{nameof(type)} cannot be null.");
            }

            if (ids == null)
            {
                throw new ArgumentNullException($"{nameof(ids)} cannot be null.");
            }

            var uriBuilder = new UriBuilder(GetObjectUri);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add("Resource", resource);
            query.Add("Type", type);
            query.Add("ID", string.Join(',', ids.Select(x => x.ToString())));
            query.Add("Location", useLocation ? "1" : "0");

            uriBuilder.Query = query.ToString();

            return await Client.Get(uriBuilder.Uri, async (response) =>
            {
                string responseContentType = response.Content.Headers.ContentType.ToString(); // GetValues("Content-Type").FirstOrDefault();

                var files = new List<FileObject>();

                if (!ContentType.TryParse(responseContentType, out ContentType documentContentType))
                {
                    return files;
                }

                using (Stream memoryStream = await GetStream(response))
                {
                    if (documentContentType.MediaSubtype.Equals("xml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // At this point we know there is a problem because Mime response is expected not XML.
                        XDocument doc = XDocument.Load(memoryStream);

                        AssertValidReplay(doc.Root);

                        return files;
                    }

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

        public async Task Disconnect()
        {
            await Client.Get(LogoutUri, _Resource?.Cookie, _Resource?.SessionId);
            _Resource = null;
        }



        protected async Task<T> ParseMetadata<T>(HttpResponseMessage response)
            where T : class, IRetsCollectionXElementLoader
        {
            using (Stream stream = await GetStream(response))
            {
                XDocument doc = XDocument.Load(stream);

                AssertValidReplay(doc.Root);

                XNamespace ns = doc.Root.GetDefaultNamespace();

                T collection = (T)Activator.CreateInstance(typeof(T));

                XElement metaData = doc.Descendants(ns + "METADATA").FirstOrDefault();

                if (metaData != null)
                {
                    // INSTEAD OF FirstOrDefault
                    // loop over all the elements
                    XElement metaDataNode = metaData.Elements().FirstOrDefault();
                    if (metaDataNode != null)
                    {
                        collection.Load(metaDataNode);
                    }
                }

                return collection;
            }
        }


        protected char GetCompactDelimiter(XDocument doc)
        {
            XNamespace ns = doc.Root.GetDefaultNamespace();
            XElement delimiter = doc.Descendants(ns + "DELIMITER").FirstOrDefault();

            if (delimiter == null)
            {
                throw new RetsParsingException("Unable to find the delimiter! Only 'COMPACT' or 'COMPACT-DECODED' are supported when querying the data");
            }

            var delimiterAttribute = delimiter.Attribute("value");

            if (delimiterAttribute != null && int.TryParse(delimiterAttribute.Value, out int value))
            {
                return Convert.ToChar(value);
            }

            return Convert.ToChar(9);
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
                ContentType = new System.Net.Mime.ContentType(message.ContentType.MimeType),
                ContentDescription = message.Headers["Content-Description"],
                ContentSubDescription = message.Headers["Content-Sub-Description"],
                ContentLocation = message.ContentLocation,
                MemeVersion = message.Headers["MIME-Version"],
                Extension = MimeTypeMap.GetExtension(message.ContentType.MimeType)
            };

            if (int.TryParse(message.Headers["Object-ID"], out int objectId))
            {
                file.ObjectId = objectId;
            }
            else
            {
                // This should never happen
                throw new RetsParsingException("For some reason Object-ID does not exists in the response or it is not an integer value as expected");
            }

            if (bool.TryParse(message.Headers["Preferred"], out bool isPreferred))
            {
                file.IsPreferred = isPreferred;
            }

            if (message.ContentLocation == null)
            {
                file.Content = new MemoryStream();
                message.Content.DecodeTo(file.Content);
                file.Content.Position = 0; // This is important otherwise the next seek with start at the end
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

        protected int GetReplayCode(XElement root)
        {
            var replayCode = root.Attribute("ReplyCode");

            if (replayCode != null && int.TryParse(replayCode.Value, out int code))
            {
                return code;
            }

            if (replayCode == null)
            {
                Log?.LogError("Unable to find ReplyCode attribute on the XElement.");
            }

            return int.MaxValue;
        }

        protected void AssertValidReplay(XElement root)
        {
            int code = GetReplayCode(root);

            AssertValidReplay(root, code);
        }

        private void AssertValidReplay(XElement root, int code)
        {
            if (!IsValidCode(code))
            {
                var replayText = root.Attribute("ReplyText");
                string message = replayText?.Value ?? "Unknown error";

                Log?.LogWarning(message);

                if (code == 20210)
                {
                    throw new TooManyOutstandingQueries(message);
                }

                if (code == 20512 || code == 20412)
                {
                    throw new TooManyOutstandingRequests(message);
                }

                throw new RetsException(message);
            }
        }

        protected bool IsValidCode(int code)
        {
            // 20201 - No records found.
            // 20403 - No objects found
            // 0 - Success

            return code == 0 || code == 20201 || code == 20403;
        }
    }
}
