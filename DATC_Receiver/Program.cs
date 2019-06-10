using Akka.Actor;
using Akka.Cluster.Sharding;
using DATC_Receiver.Actors;
using System;

namespace DATC_Receiver
{
    class Program
    {
        internal static IActorRef tower;  // where the data requests are sent to
        static void Main(string[] args)
        {
            // cluster configuration 
            var hocon = @"akka {
                            actor.provider = cluster
                            remote {
                                dot-netty.tcp {
                                    port = 8081
                                    hostname = localhost
                                }
                            }
                            cluster {
                                seed-nodes = [""akka.tcp://DATCRs@localhost:8081""]
                            }
            }";

            var confg = Akka.Configuration.ConfigurationFactory.ParseString(hocon);
            // start up akka system
            using (var system = ActorSystem.Create("DATCRs", confg))
            {
                // flight data
                tower = system.ActorOf<ControlTowerActor>("Tower");
                // cluster/eventHub manager
                var coord = system.ActorOf<CoordinatorActor>("Coord");

                // good to go!
                Console.WriteLine("<Enter> to shutdown");
                Console.ReadLine();
                Console.WriteLine("<Enter> again");
                Console.ReadLine();
            };
        }
    }
}