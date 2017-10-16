using Akka.Actor;
using Elasticsearch.GridFS;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsStorate.Api.Actors
{
    public class DownloadFileActor : ReceiveActor
    {
        public DownloadFileActor()
        {
            Receive<DownloadMessage>(x => Download(x));
            ReceiveAsync<StreamDownloadMessage>(StreamDownload);
        }

        private async Task StreamDownload(StreamDownloadMessage message)
        {
            var elkUrl = new Uri("http://localhost:9200/");
            var connection = new ConnectionSettings(elkUrl);
            var bucket = new GridFSBucket(connection);
            var stopWatch = Stopwatch.StartNew();

            Console.WriteLine($"Downloading file {message.FileName}...");
            using (var fileStream = File.Create(string.Concat(@"C:\files\", message.FileName)))
            {
                await bucket.DownloadToStreamAsync(message.DocId, fileStream);
            }
            stopWatch.Stop();
            Console.WriteLine($"The {message.FileName} file was downloaded. Elapsed time: {stopWatch.ElapsedMilliseconds} ms.");
        }

        private void Download(DownloadMessage obj)
        {
            var elkUrl = new Uri("http://localhost:9200/");
            var connection = new ConnectionSettings(elkUrl);
            var client = new ElasticClient(connection.DefaultIndex("policies"));

            var response = client.Get(new DocumentPath<Policy>(new Id(obj.DocId)));

            using (var stream = new FileStream(Path.Combine(@"C:\downloadedfiles", obj.FileName), FileMode.Create))
            {
                Console.WriteLine($"Saving the file { obj.FileName} to disk.");
                foreach(var b in response.Source.Data)
                {
                    stream.WriteByte(b);
                }
                Console.WriteLine($"Finished to save the file { obj.FileName} to disk.");
            }
        }
    }

    public class DownloadMessage
    {
        public string DocId { get; internal set; }
        public string FileName { get; internal set; }
    }

    public class StreamDownloadMessage
    {
        public Guid DocId { get; set; }
        public string FileName { get; internal set; }
    }
}
