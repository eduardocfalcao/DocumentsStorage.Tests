using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.GridFS
{
    public class FileChunckEnumerable : IEnumerable<FileChunck>
    {
        public FileChunckEnumerable(ConnectionSettings connectionSettings, Guid fileId, string indexName)
        {
            ConnectionSettings = connectionSettings;
            FileId = fileId;
            IndexName = indexName;
        }

        public ConnectionSettings ConnectionSettings { get; }
        public Guid FileId { get; }
        public string IndexName { get; }

        public IEnumerator<FileChunck> GetEnumerator()
        {
            return new FileChuncksEnumerator(ConnectionSettings, FileId, IndexName);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }
    }
 }
