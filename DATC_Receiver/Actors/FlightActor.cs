using Akka.Actor;
using Akka.Persistence;
using DATC_Receiver.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DATC_Receiver.Actors
{
    /// <summary>
    /// Process just one 1 flight code
    /// </summary>
    class FlightActor : ReceiveActor
    {
        IActorRef icaoLookup;
        IActorRef dataSaver;
        string flightID;
        FlightDataSnapshotArchive snaps;
        FlightDataSnapshot currentSnapshot;

        public FlightActor()
        {
            snaps = new FlightDataSnapshotArchive();

            Receive<FlightActorInit>(r =>
            {
                flightID = r.flightId.Trim();
                icaoLookup = r.icao;
                dataSaver = r.saver;
            });
            
            // if a common aircraft, then more details in here
            Receive<ICAOLookupActor.AircraftResponse>(r =>
            {
                snaps.icaoAircraft = r.ICAOAircraft;
            });

            // info about the plane
            Receive<ICAOLookupActor.HexResponse>(r =>
            {
                snaps.hex = r.Hex;
                snaps.icaoData = r.ICAOData;
                if (snaps.icaoData != null)
                {
                    if (!string.IsNullOrWhiteSpace(snaps.icaoData.t))
                        icaoLookup.Tell(new ICAOLookupActor.AircraftRequest(r.ICAOData.t));
                }
            });

            // this is the data from the ADS-B receiver 
            Receive<FlightDataRequest>(r =>
            {
                if (string.IsNullOrWhiteSpace(snaps.hex))
                {
                    icaoLookup.Tell(new ICAOLookupActor.HexRequest(r.flightData.hex));
                }

                // ensure that current reading is newer than previous
                // as some packets come in out of order
                bool isOutOrder = false;
                if (currentSnapshot != null && currentSnapshot.now > r.now)
                {
                    Console.WriteLine("not new came in " + snaps.flightCode);
                    isOutOrder = true;
                }

                // create snapshot for basic web view 
                buildSnapshot(r.now, r.flightData);

                // make sure in time order
                snaps.archive.Add(currentSnapshot);
                snaps.archive = snaps.archive.OrderBy(z => z.now).ToList();

                // only update status if this is newer than previous
                if (!isOutOrder)
                {                    
                    dataSaver.Tell(new CosmosSaveActor.SaveRequest(currentSnapshot, "flights"));
                }                

                // full object info - incase web user wants to see 
                var ex = new FlightDataExtended(r.flightData)
                {
                    altDelta = currentSnapshot.altDelta,
                    spdDelta = currentSnapshot.spdDelta,
                    icoaAircraft = snaps.icaoAircraft,
                    icoaData = snaps.icaoData,
                };
                dataSaver.Tell(new CosmosSaveActor.SaveRequest(ex, "flights"));
            });
        }        
        
        public static Props Props() => Akka.Actor.Props.Create(() => new FlightActor());
        

        private void buildSnapshot(double now, FlightData entry)
        {
            if (currentSnapshot == null)
            {
                // create initial snapshot
                currentSnapshot = new FlightDataSnapshot()
                {
                    id = "activeSnap:" + this.flightID,
                    flight = flightID,
                    now = now,
                    spdDelta = 0,
                    altDelta = 0,
                    spd = entry.gs ?? -1,
                    lat = entry.lat ?? -9999,
                    lon = entry.lon ?? -9999,
                    alt = entry.alt_baro,
                    track = (entry.track != null ? entry.track.Value : 0),
                };
            }
            else
            {
                var curSpd = currentSnapshot.spd;
                var curAlt = currentSnapshot.alt;

                // use previous snaphsot to get delta values
                currentSnapshot = new FlightDataSnapshot()
                {
                    id = "activeSnap:" + this.flightID,
                    flight = flightID,
                    now = now,
                    spdDelta = (entry.gs.HasValue ? entry.gs.Value - curSpd : -1),
                    altDelta = entry.alt_baro - curAlt,
                    spd = entry.gs ?? -1,
                    lat = entry.lat ?? -9999,
                    lon = entry.lon ?? -9999,
                    alt = entry.alt_baro,
                    track = (entry.track != null ? entry.track.Value : 0),
                };
            }
        }

        internal class FlightActorInit
        {
            public FlightActorInit(IActorRef saveActor, string flight, IActorRef icaoActor )
            {
                saver = saveActor;
                flightId = flight;
                icao = icaoActor;
            }
            public IActorRef saver { get; private set; }
            public string flightId { get; private set; }
            public IActorRef icao { get; private set; }
        }

        internal class FlightDataRequest
        {           
            public FlightData flightData { get; set; }
            public string deviceId { get; set; }
            public double now { get; set; }
        }
    }
}
