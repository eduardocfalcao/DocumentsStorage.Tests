using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.GridFS.Streams
{
    internal abstract class GridFSDownloadStreamBase : GridFSDownloadStream
    {
        // private fields
        private readonly GridFSBucket _bucket;
        private bool _disposed;
        private readonly FileDocument _fileInfo;

        // constructors
        protected GridFSDownloadStreamBase(
            GridFSBucket bucket,
            FileDocument fileInfo)
        {
            _bucket = bucket;
            _fileInfo = fileInfo;
        }

        // public properties
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override FileDocument FileInfo
        {
            get { return _fileInfo; }
        }

        public override long Length
        {
            get { return _fileInfo.Length; }
        }

        // protected properties

        protected GridFSBucket Bucket
        {
            get { return _bucket; }
        }

        // public methods
        public override void Close()
        {
            Close(CancellationToken.None);
        }

        public override void Close(CancellationToken cancellationToken)
        {
            base.Close();
        }

        public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            base.Close();
            return Task.FromResult(true);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        // protected methods
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected virtual void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
