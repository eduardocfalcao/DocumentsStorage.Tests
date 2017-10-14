using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.GridFS
{
    public class GridFSDownloadOptions
    {
        public bool? CheckMD5 { get; set; }

        public bool? Seekable { get; set; }
    }
}
