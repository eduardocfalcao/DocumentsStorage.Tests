using System;

namespace Elasticsearch.GridFS
{
    public class GridFSBucketOptions
    {
        public string BucketName { get; }

        public int ChunkSizeBytes { get; }
    }

    public class ImmutableGridFSBucketOptions
    {
        public ImmutableGridFSBucketOptions()
        {
            BucketName = "fs";
            ChunkSizeBytes = 255 * 1024;
        }

        public ImmutableGridFSBucketOptions(GridFSBucketOptions options)
        {
            BucketName = options.BucketName;
            ChunkSizeBytes = options.ChunkSizeBytes;
        }

        public static ImmutableGridFSBucketOptions Defauls() => new ImmutableGridFSBucketOptions();

        public string BucketName { get; }

        public int ChunkSizeBytes { get; }

        public string ChuncksindexName => string.Concat(BucketName, ".", "chuncks");
    }
}
