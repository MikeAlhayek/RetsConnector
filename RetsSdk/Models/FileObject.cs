using System;
using System.IO;
using System.Net.Mime;

namespace CrestApps.RetsSdk.Models
{
    public class FileObject : IDisposable
    {
        public string ContentId { get; set; }
        public int ObjectId { get; set; }
        public ContentType ContentType { get; set; }
        public string ContentDescription { get; set; }
        public string ContentSubDescription { get; set; }
        public Uri ContentLocation { get; set; }
        public string MemeVersion { get; set; }
        public bool IsPreferred { get; set; }
        public string Extension { get; set; }
        public Stream Content { get; set; }
        private bool IsDisposed;

        public bool IsImage => ContentType?.MediaType.StartsWith("image", StringComparison.CurrentCultureIgnoreCase) ?? false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetStream(Stream stream)
        {
            Content = stream;

            if (stream != null)
            {
                Content.Position = 0;
            }
        }

        public Stream GetStream()
        {
            return Content;
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing && Content != null)
            {
                Content.Close();
                Content.Dispose();
            }

            IsDisposed = true;
        }
    }
}
