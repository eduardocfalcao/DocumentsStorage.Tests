using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.GridFS
{
    public class GridFSUploadOptions
    {
        public int? ChunkSizeBytes { get; }

        public object Metadata { get; set; }

        public int? BatchSize { get; set; }
    }
}
