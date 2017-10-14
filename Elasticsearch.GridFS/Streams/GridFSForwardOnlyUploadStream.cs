using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.GridFS.Streams
{
    internal class GridFSForwardOnlyUploadStream : GridFSUploadStream
    {
        #region static
        // private static fields
        private static readonly Task __completedTask = Task.FromResult(true);
        #endregion

        // fields
        private bool _aborted;
        private List<byte[]> _batch;
        private long _batchPosition;
        private int _batchSize;
        private readonly GridFSBucket _bucket;
        private readonly int _chunkSizeBytes;
        private bool _closed;
        private bool _disposed;
        private readonly string _filename;
        private readonly Guid _id;
        private long _length;
        private readonly IncrementalMD5 _md5;
        //private readonly BsonDocument _metadata;

        // constructors
        public GridFSForwardOnlyUploadStream(
            GridFSBucket bucket,
            Guid id,
            string filename,
            //BsonDocument metadata,
            int chunkSizeBytes,
            int batchSize)
        {
            _bucket = bucket;
            _id = id;
            _filename = filename;
            //_metadata = metadata; // can be null
            _chunkSizeBytes = chunkSizeBytes;
            _batchSize = batchSize;

            _batch = new List<byte[]>();
            //_md5 = IncrementalMD5.Create();
        }

        // properties
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override Guid Id
        {
            get { return _id; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _length;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        // methods
        public override void Abort(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_aborted)
            {
                return;
            }
            ThrowIfClosedOrDisposed();
            _aborted = true;

            //var operation = CreateAbortOperation();
            //operation.Execute(_binding, cancellationToken);
        }

        public override async Task AbortAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_aborted)
            {
                return;
            }
            ThrowIfClosedOrDisposed();
            _aborted = true;

            //var operation = CreateAbortOperation();
            //await operation.ExecuteAsync(_binding, cancellationToken).ConfigureAwait(false);
        }

        public override void Close()
        {
            Close(CancellationToken.None);
        }

        public override void Close(CancellationToken cancellationToken)
        {
            if (_closed)
            {
                return;
            }
            ThrowIfDisposed();
            _closed = true;

            if (!_aborted)
            {
                WriteFinalBatch(cancellationToken);
                WriteFilesCollectionDocument(cancellationToken);
            }

            base.Close();
        }

        public override async Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_closed)
            {
                return;
            }
            ThrowIfDisposed();
            _closed = true;

            if (!_aborted)
            {
                await WriteFinalBatchAsync(cancellationToken).ConfigureAwait(false);
                await WriteFilesCollectionDocumentAsync(cancellationToken).ConfigureAwait(false);
            }

            base.Close();
        }

        public override void Flush()
        {
            // do nothing
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            // do nothing
            return __completedTask;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfAbortedClosedOrDisposed();
            while (count > 0)
            {
                var chunk = GetCurrentChunk(CancellationToken.None);
                var partialCount = Math.Min(count, chunk.Count);
                Buffer.BlockCopy(buffer, offset, chunk.Array, chunk.Offset, partialCount);
                offset += partialCount;
                count -= partialCount;
                _length += partialCount;
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfAbortedClosedOrDisposed();
            while (count > 0)
            {
                var chunk = await GetCurrentChunkAsync(cancellationToken).ConfigureAwait(false);
                var partialCount = Math.Min(count, chunk.Count);
                Buffer.BlockCopy(buffer, offset, chunk.Array, chunk.Offset, partialCount);
                offset += partialCount;
                count -= partialCount;
                _length += partialCount;
            }
        }

        // private methods

        //TODO Analyze how to implement this later
        //private BulkMixedWriteOperation CreateAbortOperation()
        //{
        //    var chunksCollectionNamespace = _bucket.GetChunksCollectionNamespace();
        //    var filter = new BsonDocument("files_id", _idAsBsonValue);
        //    var deleteRequest = new DeleteRequest(filter) { Limit = 0 };
        //    var requests = new WriteRequest[] { deleteRequest };
        //    var messageEncoderSettings = _bucket.GetMessageEncoderSettings();
        //    return new BulkMixedWriteOperation(chunksCollectionNamespace, requests, messageEncoderSettings)
        //    {
        //        WriteConcern = _bucket.Options.WriteConcern
        //    };
        //}

        private FileDocument CreateFilesCollectionDocument()
        {
            var uploadDateTime = DateTime.UtcNow;

            return new FileDocument
            {
                Id = _id ,
                Length = _length ,
                ChunkSize = _chunkSizeBytes,
                UploadDate = uploadDateTime,
                //Hash = HashUtils.ToHexString(_md5.GetHashAndReset()),
                FileName = _filename,
            };
        }

        private IEnumerable<FileChunck> CreateWriteBatchChunkDocuments()
        {
            var chunkDocuments = new List<FileChunck>();

            var n = (int)(_batchPosition / _chunkSizeBytes);
            foreach (var chunk in _batch)
            {
                var chunkDocument = new FileChunck
                {
                    Id = Guid.NewGuid(),
                    FilesId = _id,
                    N = n++ ,
                    Data = chunk
                };
                chunkDocuments.Add(chunkDocument);

                _batchPosition += chunk.Length;
                //_md5.AppendData(chunk, 0, chunk.Length); //TODO CALCULATE THE MD5 or HASH
            }

            return chunkDocuments;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    if (_md5 != null)
                    {
                        _md5.Dispose();
                    }

                   //_binding.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public IElasticClient GetClient(string suffix)
        {
            var connectionSettings = _bucket.ConnectionSettings;
            var indexName = _bucket.Options.BucketName + "." + suffix;
            return new ElasticClient(connectionSettings.DefaultIndex(indexName));
        }
        
        private bool IndexItems<T>(string suffix, IEnumerable<T> items)
            where T : class
        {
            var client = GetClient(suffix);

            var response = client.IndexMany(items);
            return response.IsValid;
        }

        private async Task<bool> IndexItemsAsync<T>(string suffix, IEnumerable<T> items)
           where T : class
        {
            var client = GetClient(suffix);

            var response = await client.IndexManyAsync(items);
            return response.IsValid;
        }

        private ArraySegment<byte> GetCurrentChunk(CancellationToken cancellationToken)
        {
            var batchIndex = (int)((_length - _batchPosition) / _chunkSizeBytes);

            if (batchIndex == _batchSize)
            {
                WriteBatch(cancellationToken);
                _batch.Clear();
                batchIndex = 0;
            }

            return GetCurrentChunkSegment(batchIndex);
        }

        private async Task<ArraySegment<byte>> GetCurrentChunkAsync(CancellationToken cancellationToken)
        {
            var batchIndex = (int)((_length - _batchPosition) / _chunkSizeBytes);

            if (batchIndex == _batchSize)
            {
                await WriteBatchAsync(cancellationToken).ConfigureAwait(false);
                _batch.Clear();
                batchIndex = 0;
            }

            return GetCurrentChunkSegment(batchIndex);
        }

        private ArraySegment<byte> GetCurrentChunkSegment(int batchIndex)
        {
            if (_batch.Count <= batchIndex)
            {
                _batch.Add(new byte[_chunkSizeBytes]);
            }

            var chunk = _batch[batchIndex];
            var offset = (int)(_length % _chunkSizeBytes);
            var count = _chunkSizeBytes - offset;
            return new ArraySegment<byte>(chunk, offset, count);
        }

        private void ThrowIfAbortedClosedOrDisposed()
        {
            if (_aborted)
            {
                throw new InvalidOperationException("The upload was aborted.");
            }
            ThrowIfClosedOrDisposed();
        }

        private void ThrowIfClosedOrDisposed()
        {
            if (_closed)
            {
                throw new InvalidOperationException("The stream is closed.");
            }
            ThrowIfDisposed();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void TruncateFinalChunk()
        {
            var finalChunkSize = (int)(_length % _chunkSizeBytes);
            if (finalChunkSize > 0)
            {
                var finalChunk = _batch[_batch.Count - 1];
                if (finalChunk.Length != finalChunkSize)
                {
                    var truncatedFinalChunk = new byte[finalChunkSize];
                    Buffer.BlockCopy(finalChunk, 0, truncatedFinalChunk, 0, finalChunkSize);
                    _batch[_batch.Count - 1] = truncatedFinalChunk;
                }
            }
        }

        private void WriteBatch(CancellationToken cancellationToken)
        {
            var chunkDocuments = CreateWriteBatchChunkDocuments();
            IndexItems("chuncks", chunkDocuments);
            _batch.Clear();
        }

        private async Task WriteBatchAsync(CancellationToken cancellationToken)
        {
            var chunkDocuments = CreateWriteBatchChunkDocuments();
            await IndexItemsAsync("chuncks", chunkDocuments);
            _batch.Clear();
        }

        private void WriteFilesCollectionDocument(CancellationToken cancellationToken)
        {
            var client = GetClient("files");
            var filesCollectionDocument = CreateFilesCollectionDocument();
            client.Index(filesCollectionDocument);
        }

        private async Task WriteFilesCollectionDocumentAsync(CancellationToken cancellationToken)
        {
            var client = GetClient("files");
            var filesCollectionDocument = CreateFilesCollectionDocument();
            await client.IndexAsync(filesCollectionDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private void WriteFinalBatch(CancellationToken cancellationToken)
        {
            if (_batch.Count > 0)
            {
                TruncateFinalChunk();
                WriteBatch(cancellationToken);
            }
        }

        private async Task WriteFinalBatchAsync(CancellationToken cancellationToken)
        {
            if (_batch.Count > 0)
            {
                TruncateFinalChunk();
                await WriteBatchAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
