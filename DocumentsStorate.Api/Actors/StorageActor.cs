using Akka.Actor;
using Akka.DI.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
