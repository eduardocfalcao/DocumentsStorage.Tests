using Akka.Actor;
using Akka.DI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsStorate.Api.Actors
{
    public class StorageActor : ReceiveActor
    {
        IActorRef IndexFileActor { get; }
        IActorRef StoreFileActor { get; }
        public StorageActor()
        {
            StoreFileActor = Context.ActorOf(Context.DI().Props<StoreFileActor>());
            IndexFileActor = Context.ActorOf(Context.DI().Props<IndexFileActor>());

            Receive<FileStoreMessage>(x => Process(x));
        }

        private void Process(FileStoreMessage message)
        {
            IndexFileActor.Tell(message);
            StoreFileActor.Tell(message);
        }
    }

    public class StoreFileActor : ReceiveActor
    {
        public StoreFileActor()
        {
            ReceiveAsync<FileStoreMessage>(Process);
        }

        private async Task Process(FileStoreMessage message)
        {
            throw new NotImplementedException();
        }
    }

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
            Console.WriteLine($"Indexing ");
            using (var stream = new MemoryStream(message.FileContent))
            {
                var client = new HttpClient();
                var indexDocUri = new Uri(BaseUri, $"update/extract?literal.id={message.FileMetadata.Key}&commit={true}");
                var streamContent = new StreamContent(stream);
                var docIndexingResponse = await client.PostAsync(indexDocUri, streamContent);
            }
        }
    }
}
