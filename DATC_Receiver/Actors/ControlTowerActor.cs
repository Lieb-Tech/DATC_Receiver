using Akka.Actor;
using Akka.Cluster.Sharding;
using DATC_Receiver.DataStructures;
using DATC_Receiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DATC_Receiver.Actors
{
    /// <summary>
    ///  Accept device reading from clients, and distribute to flight actors
    /// </summary>
    class ControlTowerActor : ReceiveActor
    {
        IActorRef region;

        // keep a list of actors, so we don't init after each message; and when the last message is accepted for aging 
        Dictionary<string, DateTime> initedActors = new Dictionary<string, DateTime>();

        public ControlTowerActor()
        {
            // CosmosDb methods
            var cdb = new CosmosDB();
            // get info about the flight 
            var icao = Context.ActorOf<ICAOLookupActor>();

            // register actor type as a sharded entity
            region = ClusterSharding.Get(Context.System).Start(
                typeName: "FlightActor",
                entityProps: Props.Create<FlightActor>(),
                settings: ClusterShardingSettings.Create(Context.System),
                messageExtractor: new MessageExtractor());

            // get a set of data readings
            Receive<DeviceReading>(r => 
            {
                foreach (var a in r.aircraft.Where(z => !string.IsNullOrWhiteSpace(z.flight)))
                {
                    if (!initedActors.ContainsKey(a.flight))
                    {
                        initedActors.Add(a.flight, DateTime.Now);
                        var cos = Context.ActorOf(CosmosSaveActor.Props(cdb));
                        region.Tell(new ShardEnvelope(shardId: "1", entityId: a.flight, message: new FlightActor.FlightActorInit(cos, a.flight, icao)));
                    }                    
                    // create message for flight actor
                    var req = new FlightActor.FlightDataRequest()
                    {
                        deviceId = r.deviceId,
                        flightData = a,
                        now = r.now
                    };

                    // send message to entity through shard region
                    region.Tell(new ShardEnvelope(shardId: "1", entityId: a.flight, message: req));
                    initedActors[a.flight] = DateTime.Now;
                }
            });
        }

        // define envelope used to message routing
        public sealed class ShardEnvelope
        {
            public ShardEnvelope(string shardId, string entityId, object message)
            {
                ShardId = shardId ;
                EntityId = entityId ;
                Message = message;
            }
            public readonly string ShardId;
            public readonly string EntityId;
            public readonly object Message;
        }

        // define, how shard id, entity id and message itself should be resolved
        public sealed class MessageExtractor : IMessageExtractor
        {
            public string EntityId(object message) => (message as ShardEnvelope)?.EntityId.ToString();
            public string ShardId(object message) => (message as ShardEnvelope)?.ShardId.ToString();
            public object EntityMessage(object message) => (message as ShardEnvelope)?.Message;
        }
    }
}
