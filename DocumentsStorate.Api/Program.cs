using Akka.Actor;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Autofac;
using Autofac.Integration.WebApi;
using DocumentsStorate.Api.Actors;
using Elasticsearch.GridFS;
using Microsoft.Owin.Hosting;
using Nest;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace DocumentsStorate.Api
{
    public class  Policy
    {
        public Guid Id { get; set; }

        [Text(Name = "filename")]
        public string FileName { get; set; }

        public byte[] Data { get; set; }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            DownloadStreamTests();

            Console.ReadKey();
        }

        public static void DownloadStreamTests()
        {
            var fileId = Guid.Parse("04d8ac7b-a08c-4b83-90b6-7c006846c829");

            var elkUrl = new Uri("http://localhost:9200/");
            var connection = new ConnectionSettings(elkUrl);
            var bucket = new GridFSBucket(connection);

            Console.WriteLine("Downloading file...");
            using (var fileStream = File.Create(@"C:\files\testdoc0.docx"))
            {
                bucket.DownloadToStreamAsync(fileId, fileStream).Wait();
            }
            Console.WriteLine("The file was downloaded.");
        }

        public static void UploadTests()
        {
            var elkUrl = new Uri("http://localhost:9200/");
            var connection = new ConnectionSettings(elkUrl);
            var bucket = new GridFSBucket(connection);
            
            using (var fileStream = File.OpenRead(@"D:\files\testdoc0.docx"))
            {
                bucket.UploadFromStreamAsync(Guid.NewGuid(), "testdoc0.docx", fileStream).Wait();
            }
        }

        public static void DownloadTests()
        {
            var system = ActorSystem.Create("FilStorageSystem");
            var routerActor = system.ActorOf(Props.Create<RouterActor>());

            for (int i = 0; i < 10; i++)
            {
                var elkUrl = new Uri("http://localhost:9200/");
                var connection = new ConnectionSettings(elkUrl);
                var client = new ElasticClient(connection.DefaultIndex("policies"));

                var result = client.Search<Policy>(s => s
                    .Sort(sort => sort.Ascending(a => a.FileName))
                    .From(i * 100)
                    .Size(100)
                    .Source(sf => sf
                        .Includes(inc => inc
                            .Fields(
                                f => f.FileName,
                                f => f.Id
                            )
                        )
                    )
                );

                Console.WriteLine($"The query was returned {result.Documents.Count} documents to the page {i}");

                foreach (var document in result.Documents)
                {
                    routerActor.Tell(new DownloadMessage() { DocId = document.Id.ToString(), FileName = document.FileName });
                }
            }

            Console.WriteLine($"All files are sent to download. They are downloading in background.");
        }
    }

    public class Startup
    {
        private readonly ILifetimeScope _container;

        private readonly ActorSystem _actorSystem;

        public Startup()
        {
            _actorSystem = ActorSystem.Create("FilStorageSystem");
            var routerActor = _actorSystem.ActorOf(_actorSystem.DI().Props<RouterActor>());
            _container = GetContainer();
            new AutoFacDependencyResolver(_container, _actorSystem);
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration()
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(_container)
            };

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }

        private ILifetimeScope GetContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).InstancePerRequest();

            builder.RegisterInstance(_actorSystem)
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<ActorSystemWrapper>().As<IActorSystem>();

            return builder.Build();
        }
    }

    public interface IActorSystem
    {
        IActorRef ActorOf(Props props, string name = null);
        IActorRef ActorOf<TActor>(string name = null) where TActor : ActorBase, new();
        ActorSelection ActorSelection(string actorPath);
        ActorSelection ActorSelection(ActorPath actorPath);
        void Dispose();
        void PublishMessage(object message);
    }

    public class ActorSystemWrapper : IActorSystem, IDisposable
    {
        public ActorSystem ActorSystem { get; }

        public ActorSystemWrapper(ActorSystem actorSystem)
        {
            ActorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
        }

        public IActorRef ActorOf(Props props, string name = null)
        {
            return ActorSystem.ActorOf(props, name);
        }

        public IActorRef ActorOf<TActor>(string name = null) where TActor : ActorBase, new()
        {
            return ActorSystem.ActorOf<TActor>("name");
        }

        public ActorSelection ActorSelection(string actorPath)
        {
            return ActorSystem.ActorSelection(actorPath);
        }

        public ActorSelection ActorSelection(ActorPath actorPath)
        {
            return ActorSystem.ActorSelection(actorPath);
        }

        public void Dispose()
        {
            ActorSystem.Dispose();
        }

        public void PublishMessage(object message)
        {
            ActorSystem.EventStream.Publish(message);
        }
    }

}
