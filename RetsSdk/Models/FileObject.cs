using System;
using System.IO;
using System.Net.Mime;

namespace RetsSdk.Models
{
    public class FileObject : IDisposable
    {
        public string ContentId { get; set; }
        public string ObjectId { get; set; }
        public ContentType ContentType { get; set; }
        public string ContentDescription { get; set; }
        public string ContentSubDescription { get; set; }
        public Uri ContentLocation { get; set; }
        public string MemeVersion { get; set; }
        public bool IsPreferred { get; set; }
        public string Extension { get; set; }
        public MemoryStream Content { get; set; }
        private bool IsDisposed;

        public string MakeFileName()
        {
            if(string.IsNullOrWhiteSpace(Extension))
            {
                throw new Exception($"{Extension} cannot be null");
            }

            return string.Format("{0}_{1}{2}", ContentId, ObjectId, Extension);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
