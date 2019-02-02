using RetsSdk.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RetsSdk.Services
{
    public class Session
    {
        private ConnectionOptions Options;
        private RetsRequester Requester;

        public Session(ConnectionOptions options)
        {
            Options = options ?? throw new NullReferenceException($"{options} cannot be null.");
            Requester = new RetsRequester(Options);
        }

        public async Task Connect()
        {
            await Requester.LoginRequestAsync();
        }

        public async Task Disconnect()
        {
            await Requester.LogoutRequest();
        }

        public async Task<IEnumerable<FileObject>> GetObject(string resource, string type, PhotoId id, bool useLocation = false)
        {
            return await Requester.GetObject(resource, type, id, useLocation);
        }

        public async Task Search(SearchRequest request)
        {
            await Requester.Search(request);
        }


        public async Task<RetsSystem> GetSystemMetadata()
        {
            // Do something filter by resourceId
            return await Requester.GetSystemMetadata();
        }

        public async Task<RetsResourceCollection> GetResourcesMetadata()
        {
            // Do something filter by resourceId
            return await Requester.GetResourcesMetadata();
        }

        public async Task<RetsResource> GetResourceMetadata(string resourceId)
        {
            // Do something filter by resourceId
            return await Requester.GetResourceMetadata(resourceId);
        }

        public async Task<RetsClassCollection> GetClassesMetadata(string resourceId)
        {
            return await Requester.GetClassesMetadata(resourceId);
        }

        public async Task<RetsObjectCollection> GetObjectMetadata(string resourceId)
        {
            return await Requester.GetObjectMetadata(resourceId);
        }


        public async Task<RetsLookupTypeCollection> GetLookupValues(string resourceId, string lookupName)
        {
            return await Requester.GetLookupValues(resourceId, lookupName);
        }

        public async Task<RetsTableCollection> GetTableMetadata(string resourceId, string lookupName)
        {
            return await Requester.GetTableMetadata(resourceId, lookupName);
        }

    }
}
