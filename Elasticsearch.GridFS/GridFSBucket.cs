using Elasticsearch.GridFS.Streams;
using Nest;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.GridFS
{
    public class GridFSBucket
    {
        public GridFSBucket(ConnectionSettings connectionSettings, GridFSBucketOptions options = null)
        {
            ConnectionSettings = connectionSettings;
            Options = options != null ? new ImmutableGridFSBucketOptions(options) : ImmutableGridFSBucketOptions.Defauls();
        }

        public ConnectionSettings ConnectionSettings { get; }

        public ImmutableGridFSBucketOptions Options { get; }

        public async Task UploadFromStreamAsync(
            Guid id, string fileName, Stream source,
            GridFSUploadOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            using (var destination = OpenUploadStream(id, fileName, options, cancellationToken))
            {
                var chunkSizeBytes = Options.ChunkSizeBytes;
                var buffer = new byte[chunkSizeBytes];

                while (true)
                {
                    int bytesRead = 0;
                    Exception sourceException = null;
                    try
                    {
                        bytesRead = source.Read(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        // cannot await in the body of a catch clause
                        sourceException = ex;
                    }
                    if (sourceException != null)
                    {
                        try
                        {
                            destination.Abort();
                        }
                        catch
                        {
                            // ignore any exceptions because we're going to rethrow the original exception
                        }
                        throw sourceException;
                    }
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                }

                await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public GridFSDownloadStream OpenDownloadStream(Guid id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            options = options ?? new GridFSDownloadOptions();

            var fileInfo = GetFileInfo(id, cancellationToken);
            return CreateDownloadStream(fileInfo, options, cancellationToken);
        }

        private FileDocument GetFileInfo(Guid id, CancellationToken cancellationToken)
        {
            var client = new ElasticClient(ConnectionSettings.DefaultIndex("files"));
            var response = client.Get(new DocumentPath<FileDocument>(new Id(id)));
            return response.Source;
        }

        private GridFSDownloadStream CreateDownloadStream(FileDocument fileInfo, GridFSDownloadOptions options, CancellationToken cancellationToken)
        {
            var checkMD5 = options.CheckMD5 ?? false;
            var seekable = options.Seekable ?? false;

            return new GridFSForwardOnlyDownloadStream(this, binding, fileInfo, checkMD5);
        }

        private  GridFSUploadStream OpenUploadStream(Guid id, string fileName, 
            GridFSUploadOptions options = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var chunkSizeBytes = Options.ChunkSizeBytes;
            var batchSize =  (16 * 1024 * 1024 / chunkSizeBytes);

            return new GridFSForwardOnlyUploadStream(
                this,
                id,
                fileName,
                chunkSizeBytes,
                batchSize);
        }
    }
}
