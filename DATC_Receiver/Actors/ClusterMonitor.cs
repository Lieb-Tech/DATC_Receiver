using Akka.Actor;
using Akka.Cluster;
using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.Actors
{
    /// <summary>
    /// Get notification when cluster node joins
    /// </summary>
    class ClusterMonitor : UntypedActor
    {
        // get reference to cluster
        protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);
        public ClusterMonitor()
        {
            // register for notifications
            Cluster.Subscribe(Self, new[] { typeof(ClusterEvent.IMemberEvent) });
        }

        protected override void OnReceive(object message)
        {
            var up = message as ClusterEvent.MemberUp;
            // only really care about up for now
            if (up != null)
            {
                var mem = up;
                var select = Context.System.ActorSelection("akka://DATCRs/user/Coord");
                select.Tell(new CoordinatorActor.ReadyToCollect());
                Console.WriteLine(">> Member up");
            }
            else if (message is ClusterEvent.UnreachableMember)
            {
                var unreachable = (ClusterEvent.UnreachableMember)message;
                
            }
            else if (message is ClusterEvent.MemberRemoved)
            {
                var removed = (ClusterEvent.MemberRemoved)message;
                
            }
            else if (message is ClusterEvent.IMemberEvent)
            {
                //IGNORE
            }
            else
            {
                Unhandled(message);
            }
        }
    }
}
