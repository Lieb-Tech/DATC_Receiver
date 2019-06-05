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
            var cdb = new CosmosDB();            
            var icao = Context.ActorOf<ICAOLookupActor>();
            Dictionary<string, IActorRef> flightActors = new Dictionary<string, IActorRef>();
            Dictionary<string, DateTime> flightExpiry = new Dictionary<string, DateTime>();

            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15), Self, new ExpireActors(), Self);

            Receive<DeviceReading>(r =>
            {
                foreach (var dr in r.aircraft)
                {
                    var req = new FlightActor.FlightDataRequest()
                    {
                        deviceId = r.deviceId,
                        flightData = dr,
                        now = r.now
                    };
                    if (!flightActors.ContainsKey(dr.flight))
                    {
                        var cos = Context.ActorOf(CosmosSaveActor.Props(cdb));
                        flightActors.Add(dr.flight, Context.ActorOf(FlightActor.Props(cos, dr.flight, icao)));
                        flightExpiry.Add(dr.flight, DateTime.Now);
                    }

                    flightActors[dr.flight].Tell(req);
                    flightExpiry.Add(dr.flight, DateTime.Now);
                }
            });

            Receive<ExpireActors>(r =>
            {
                var toCleanup = flightExpiry.Where(z => z.Value.AddHours(1) < DateTime.Now).Select(z => z.Key).ToList();
                foreach (var t in toCleanup)
                {
                    var actor = flightActors[t];
                    actor.GracefulStop(TimeSpan.FromSeconds(10));
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
