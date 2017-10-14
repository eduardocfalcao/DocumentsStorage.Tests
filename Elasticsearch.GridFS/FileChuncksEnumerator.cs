using System;
using System.Collections;
using System.Collections.Generic;
using Nest;
using System.Linq;

namespace Elasticsearch.GridFS
{
    internal class FileChuncksEnumerator : IEnumerator<FileChunck>
    {
        private ConnectionSettings ConnectionSettings;
        private Guid FileId;
        private int _currentIndex;
        private FileChunck _currentFileChunck;

        public FileChunck Current => _currentFileChunck;
        object IEnumerator.Current => _currentFileChunck;
        public string IndexName { get; }

        public FileChuncksEnumerator(ConnectionSettings connectionSettings, Guid fileId, string indexName)
        {
            ConnectionSettings = connectionSettings;
            FileId = fileId;
            IndexName = indexName;
            _currentIndex = 0;
            _currentFileChunck = null;
        }

        public bool MoveNext()
        {
            var client = new ElasticClient(ConnectionSettings.DefaultIndex(IndexName));

            //montar a query do elastic search
            //recuperar o chunck
            //se tiver chunck, preencher em uma variável local o chunck retornado e retornar true
            //se nao tiver, retorna false
            var query = client.Search<FileChunck>(s => s
                    .Size(1)
                    .Query(q => 
                        q.Match(m => m.Field(f => f.FilesId).Query(FileId.ToString())) &&
                        q.Match(m => m.Field(f => f.N).Query(_currentIndex.ToString()))
                    )
                );

            var hasChuncks = query.Documents.Count > 0;
            if (!hasChuncks) return false;
            _currentFileChunck = query.Documents.First();
            _currentIndex++;
            return true;
        }

        public void Reset()
        {
            _currentIndex = 0;
            _currentFileChunck = null;
        }

        public void Dispose()
        {

        }
    }
}