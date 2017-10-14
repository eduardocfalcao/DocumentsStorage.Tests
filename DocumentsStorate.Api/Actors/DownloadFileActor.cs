using Akka.Actor;
using Nest;
using System;
using System.Collections.Generic;
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
}
