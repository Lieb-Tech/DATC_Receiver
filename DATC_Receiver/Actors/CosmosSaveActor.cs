using Akka.Actor;
using DATC_Receiver.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.Actors
{
    class CosmosSaveActor : ReceiveActor
    {
        CosmosDB cosmosDB;
        public CosmosSaveActor(CosmosDB cdb)
        {
            cosmosDB = cdb;

            Receive<SaveRequest>(r =>
            {
                if (r.ToSave == null)
                    Sender.Tell(new BadRequest() { BadToSave = true });
                else if (string.IsNullOrWhiteSpace(r.Collection))
                    Sender.Tell(new BadRequest() { BadCollection = true, ToSave = r.ToSave });
                else
                {
                    try
                    {
                        cosmosDB.UpsertDocument(r.ToSave, r.Collection);
                    }
                    catch (Exception ex)
                    {
                        Sender.Tell(new FailedToSave() { Exception = ex, ToSave = r.ToSave });
                    }
                }
            });
        }
        
        public static Props Props(CosmosDB cdb) =>
           Akka.Actor.Props.Create(() => new CosmosSaveActor(cdb));

        internal class FailedToSave
        {
            public object ToSave { get; set; }
            public Exception Exception { get; set; }
        }

        internal class BadRequest
        {
            public object ToSave { get; set; }
            public bool BadToSave { get; set; }
            public bool BadCollection { get; set; }
            public BadRequest()
            {
                BadToSave = false;
                BadCollection = false;
            }
        }

        internal class SaveRequest
        {
            public string Collection;
            public dynamic ToSave;
            public SaveRequest(dynamic objectToSave, string collection)
            {
                ToSave = objectToSave;
                Collection = collection;
            }
        }
    }
}
