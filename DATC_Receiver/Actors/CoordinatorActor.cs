using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.Actors
{
    /// <summary>
    /// Wait for cluster node join up; then start data stream
    /// </summary>
    class CoordinatorActor : ReceiveActor
    {
        EventHubService eh;
        public CoordinatorActor()
        {
            var a = Context.System.ActorOf<Actors.ClusterMonitor>("cluster");

            // when the first note joins, start up data stream
            Receive<ReadyToCollect>(r => 
            {
                // don't want multiple of these active 
                if (eh != null)
                {                    
                    eh = new EventHubService();
                    eh.StartReceivers();
                }
            });
        }

        internal class ReadyToCollect
        {
        }
    }
}
