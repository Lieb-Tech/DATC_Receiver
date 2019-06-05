using Akka.Actor;
using Akka.Cluster.Sharding;
using DATC_Receiver.Actors;
using System;

namespace DATC_Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("DFC"))
            {
                
            };
        }
    }
}