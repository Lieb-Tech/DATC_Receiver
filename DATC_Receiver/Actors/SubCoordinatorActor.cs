using Akka.Actor;
using Akka.Routing;
using DATC_Receiver.DataStructures;
using DATC_Receiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DATC_Receiver.Actors
{
    class SubCoordinatorActor : ReceiveActor
    {
        public SubCoordinatorActor()
        {
            // CosmosDb methods
            var cdb = new CosmosDB();            
            // get info about the flight 
            var icao = Context.ActorOf<ICAOLookupActor>();

            // flight code => Actor
            Dictionary<string, IActorRef> flightActors = new Dictionary<string, IActorRef>();
            // flight code => last message processed
            Dictionary<string, DateTime> flightExpiry = new Dictionary<string, DateTime>();

            // clear out old actors
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15), Self, new ExpireActors(), Self);

            // group of readings 
            Receive<DeviceReading>(r =>
            {
                // foreach plane in current reading -- ignoring those w/o flight code
                foreach (var dr in r.aircraft.Where(z => !string.IsNullOrWhiteSpace(z.flight)))
                {
                    // create message for flight actor
                    var req = new FlightActor.FlightDataRequest()
                    {
                        deviceId = r.deviceId,
                        flightData = dr,
                        now = r.now
                    };
                    
                    // if not started up, then do so now
                    if (!flightActors.ContainsKey(dr.flight))
                    {
                        var cos = Context.ActorOf(CosmosSaveActor.Props(cdb));
                        flightActors.Add(dr.flight, Context.ActorOf(FlightActor.Props(cos, dr.flight, icao)));
                        flightExpiry.Add(dr.flight, DateTime.Now);
                    }

                    // send message
                    flightActors[dr.flight].Tell(req);
                    // update timestamp
                    flightExpiry[dr.flight] = DateTime.Now;
                }
            });

            // clear out actors that haven't recently processed a message
            Receive<ExpireActors>(r =>
            {
                // get idle processor list
                var toCleanup = flightExpiry.Where(z => z.Value.AddHours(1) < DateTime.Now).Select(z => z.Key).ToList();
                foreach (var t in toCleanup)
                {
                    // shut down the actor
                    var actor = flightActors[t];
                    actor.GracefulStop(TimeSpan.FromSeconds(10));
                    // clear out from lists
                    flightActors.Remove(t);
                    flightExpiry.Remove(t);
                }
            });
        }

        internal class ExpireActors
        {        
        }
    }    
}
