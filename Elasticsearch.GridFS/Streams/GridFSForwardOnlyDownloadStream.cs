using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.GridFS.Streams
{
    internal class GridFSForwardOnlyDownloadStream : GridFSDownloadStreamBase
    {
        // private fields
        private FileChunck _chunck;
        private long _batchPosition;
        private readonly bool _checkMD5;
        private bool _closed;
        private IEnumerator<FileChunck> _cursor;
        private bool _disposed;
        private readonly int _lastChunkNumber;
        private readonly int _lastChunkSize;
        private readonly IncrementalMD5 _md5;
        private int _nextChunkNumber;
        private long _position;

        // constructors
        public GridFSForwardOnlyDownloadStream(
            GridFSBucket bucket,
            FileDocument fileInfo,
            bool checkMD5)
            : base(bucket, fileInfo)
        {
            _checkMD5 = checkMD5;
            if (_checkMD5)
            {
                _md5 = new IncrementalMD5();
            }

            _lastChunkNumber = (int)((fileInfo.Length - 1) / fileInfo.ChunkSize);
            _lastChunkSize = (int)(fileInfo.Length % fileInfo.ChunkSize);

            if (_lastChunkSize == 0)
            {
                _lastChunkSize = fileInfo.ChunkSize;
            }
        }

        // public properties
        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        // methods
        public override void Close(CancellationToken cancellationToken)
        {
            CloseHelper();
            base.Close(cancellationToken);
        }

        public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CloseHelper();
            return base.CloseAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();

            var bytesRead = 0;
            while (count > 0 && _position < FileInfo.Length)
            {
                var segment = GetSegment(CancellationToken.None);

                var partialCount = Math.Min(count, segment.Count);
                Buffer.BlockCopy(segment.Array, segment.Offset, buffer, offset, partialCount);

                bytesRead += partialCount;
                offset += partialCount;
                count -= partialCount;
                _position += partialCount;
            }

            return bytesRead;
        }

        //public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        //{
        //    ThrowIfDisposed();

        //    var bytesRead = 0;
        //    while (count > 0 && _position < FileInfo.Length)
        //    {
        //        var segment = await GetSegmentAsync(cancellationToken).ConfigureAwait(false);

        //        var partialCount = Math.Min(count, segment.Count);
        //        Buffer.BlockCopy(segment.Array, segment.Offset, buffer, offset, partialCount);

        //        bytesRead += partialCount;
        //        offset += partialCount;
        //        count -= partialCount;
        //        _position += partialCount;
        //    }

        //    return bytesRead;
        //}

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        // protected methods
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_cursor != null)
                    {
                        _cursor.Dispose();
                    }
                    if (_md5 != null)
                    {
                        _md5.Dispose();
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected override void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            base.ThrowIfDisposed();
        }

        // private methods
        private void CloseHelper()
        {
            if (!_closed)
            {
                _closed = true;

                if (_checkMD5 && _position == FileInfo.Length)
                {
                    var md5 = HashUtils.ToHexString(_md5.GetHashAndReset());
                    if (!md5.Equals(FileInfo.Hash, StringComparison.OrdinalIgnoreCase))
                    {
#pragma warning disable 618
                        throw new Exception($"The file don't have the persisted Hash {FileInfo.Id}");//throw new GridFSMD5Exception(_idAsBsonValue);
#pragma warning restore
                    }
                }
            }
        }

        private void GetFirstBatch(CancellationToken cancellationToken)
        {
            _cursor = new FileChunckEnumerable(Bucket.ConnectionSettings, FileInfo.Id, Bucket.Options.ChuncksindexName).GetEnumerator();
            GetNextBatch(cancellationToken);
        }

        private void GetNextBatch(CancellationToken cancellationToken)
        {
            FileChunck chunck;

            var hasChunck = _cursor.MoveNext();
            chunck = hasChunck ? _cursor.Current : null;

            ProcessNextBatch(chunck);
        }

        private void ProcessNextBatch(FileChunck batch)
        {
            if (batch == null)
            {
#pragma warning disable 618
                throw new Exception("missing - batch is null");//throw new GridFSChunkException(_idAsBsonValue, _nextChunkNumber, "missing");
#pragma warning restore
            }

            var previousBatch = _chunck;
            _chunck = batch;

            if (previousBatch != null)
            {
                _batchPosition +=  FileInfo.ChunkSize;
            }

            if (_chunck.N == _lastChunkNumber + 1 && _chunck.Data.Length == 0)
            {
                return;
            }

            var n = _chunck.N;
            var bytes = _chunck.Data;

            if (n != _nextChunkNumber)
            {
#pragma warning disable 618
                throw new Exception($"missing the chunck {n} on file {FileInfo.Id}");//throw new GridFSChunkException(_idAsBsonValue, _nextChunkNumber, "missing");
#pragma warning restore
            }
            _nextChunkNumber++;

            var expectedChunkSize = n == _lastChunkNumber ? _lastChunkSize : FileInfo.ChunkSize;
            if (bytes.Length != expectedChunkSize)
            {
#pragma warning disable 618
                throw new Exception("the wrong size");//throw new GridFSChunkException(_idAsBsonValue, _nextChunkNumber, "the wrong size");
#pragma warning restore
            }

            if (_checkMD5)
            {
                _md5.AppendData(bytes, 0, bytes.Length);
            }
        }

        private ArraySegment<byte> GetSegment(CancellationToken cancellationToken)
        {
            if (_cursor == null)
            {
                GetFirstBatch(cancellationToken);
            }
            else
            {
                GetNextBatch(cancellationToken);
            }

            return GetSegmentHelper();
        }

        //private async Task<ArraySegment<byte>> GetSegmentAsync(CancellationToken cancellationToken)
        //{
        //    var batchIndex = (int)((_position - _batchPosition) / FileInfo.ChunkSize);

        //    if (_cursor == null)
        //    {
        //        await GetFirstBatchAsync(cancellationToken).ConfigureAwait(false);
        //    }
        //    else if (batchIndex == _chunck.Count)
        //    {
        //        await GetNextBatchAsync(cancellationToken).ConfigureAwait(false);
        //        batchIndex = 0;
        //    }

        //    return GetSegmentHelper(batchIndex);
        //}

        private ArraySegment<byte> GetSegmentHelper()
        {
            var bytes = _chunck.Data;
            var segmentOffset = (int)(_position % FileInfo.ChunkSize);
            var segmentCount = bytes.Length - segmentOffset;
            return new ArraySegment<byte>(bytes, segmentOffset, segmentCount);
        }
    }
}
