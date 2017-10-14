using Akka.Actor;
using Akka.DI.Core;
using Akka.Routing;

namespace DocumentsStorate.Api.Actors
{
    public class RouterActor : ReceiveActor
    {
        IActorRef _storageActorPoll;
        IActorRef DownloadActorPoll => GetPool();

        public RouterActor()
        {
            Receive<DownloadMessage>(message => RouteeMessage(message));
        }
        
        private void RouteeMessage(DownloadMessage message)
        {
            DownloadActorPoll.Forward(message);
        }

        private IActorRef GetPool()
        {
            if(_storageActorPoll == null)
            {
                _storageActorPoll = Context.ActorOf(Props.Create<DownloadFileActor>().WithRouter(new RoundRobinPool(5)));
            }
            return _storageActorPoll;
        }
    }
}
