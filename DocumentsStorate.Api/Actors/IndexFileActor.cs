using Akka.Actor;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DocumentsStorate.Api.Actors
{
    public class IndexFileActor : ReceiveActor
    {
        Uri BaseUri { get; }
        public IndexFileActor()
        {
            BaseUri = new Uri("http://localhost:8983/solr/default/");
            ReceiveAsync<FileStoreMessage>(Process);
        }

        private async Task Process(FileStoreMessage message)
        {
            //Console.WriteLine($"Indexing ");
            //using (var stream = new MemoryStream(message.FileContent))
            //{
            //    var client = new HttpClient();
            //    var indexDocUri = new Uri(BaseUri, $"update/extract?literal.id={message.FileMetadata.Key}&commit={true}");
            //    var streamContent = new StreamContent(stream);
            //    var docIndexingResponse = await client.PostAsync(indexDocUri, streamContent);
            //}
        }
    }
}
