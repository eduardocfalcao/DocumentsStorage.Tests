using Nest;
using System;

namespace Elasticsearch.GridFS
{
    public class FileDocument
    {
        public Guid Id { get; set; }

        public long Length { get; set; }

        public int ChunkSize { get; set; }

        public DateTime UploadDate { get; set; }

        /// <summary>
        /// MD5 in specification of GridFS
        /// </summary>
        public string Hash { get; set; }

        public string FileName { get; set; }

        [Object]
        public object Metadata { get; }
    }


    public class FileChunck
    {
        public Guid Id { get; set; }
        
        public Guid FilesId { get; set; }

        public int N { get; set; }
        
        [Binary]
        public byte[] Data { get; set; }
    }
}
