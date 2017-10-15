using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Routing;

namespace DocumentsStorate.Api.Actors
{
    public class RouterActor : ReceiveActor
    {
        IActorRef _storageActorPoll;
        IActorRef _uploadActorPoll;
        IActorRef DownloadActorPoll => GetDownloadFilePool();
        IActorRef UploadActorPoll => GetUploadActorPool();

        public RouterActor()
        {
            Receive<DownloadMessage>(m => DownloadActorPoll.Forward(m));
            Receive<FileStoreMessage>(m => UploadActorPoll.Forward(m));

        }

        private IActorRef GetDownloadFilePool()
        {
            if(_storageActorPoll == null)
            {
                _storageActorPoll = Context.ActorOf(Props.Create<DownloadFileActor>().WithRouter(new RoundRobinPool(5)));
            }
            return _storageActorPoll;
        }

        private IActorRef GetUploadActorPool()
        {
            if (_uploadActorPoll == null)
            {
                _uploadActorPoll = Context.ActorOf(Props.Create<StoreFileActor>().WithRouter(new RoundRobinPool(5)));
            }
            return _uploadActorPoll;
        }
    }
}
