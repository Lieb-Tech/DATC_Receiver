using Akka.Actor;
using Akka.Cluster.Sharding;
using DATC_Receiver.Actors;
using System;

namespace DATC_Receiver
{
    class Program
    {
        internal static IActorRef subCoord;
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("DFC"))
            {
                subCoord = system.ActorOf<SubCoordinatorActor>("subCoord");

                var eh = new EventHubService();
                eh.StartReceivers();

                Console.WriteLine("<Enter> to shutdown");
                Console.ReadLine();
                Console.WriteLine("<Enter> again");
                Console.ReadLine();
            };
        }
    }
}