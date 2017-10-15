using Akka.Actor;
using Elasticsearch.GridFS;
using Nest;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentsStorate.Api.Actors
{
    public class StoreFileActor : ReceiveActor
    {
        public StoreFileActor()
        {
            ReceiveAsync<FileStoreMessage>(Process);
        }

        private async Task Process(FileStoreMessage message)
        {
            var elkUrl = new Uri("http://localhost:9200/");
            var connection = new ConnectionSettings(elkUrl);
            var bucket = new GridFSBucket(connection);

            var fileName = message.Filepath.Split('\\').Last();

            Console.WriteLine($"Uploading the file {fileName}.");
            var timer = new Stopwatch();
            timer.Start();

            using (var fileStream = File.OpenRead(message.Filepath))
            {
                await bucket.UploadFromStreamAsync(Guid.NewGuid(), fileName, fileStream);
            }
            timer.Stop();
            Console.WriteLine($"Upload of file {fileName} has finished. Elaspsed time: {timer.ElapsedMilliseconds} ms.");
        }
    }
}
