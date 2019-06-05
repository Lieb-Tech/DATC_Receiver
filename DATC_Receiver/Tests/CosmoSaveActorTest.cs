using Akka.Actor;
using Akka.TestKit.NUnit3;
using DATC_Receiver.Actors;
using DATC_Receiver.Helpers;
using Microsoft.Azure.Documents.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DATC_Receiver.Tests
{
    [TestFixture]
    class CosmoSaveActorTest : TestKit
    {
        [Test]
        public void BadObjectToSave()
        {
            var cdb = new CosmosDB();
            var act = ActorOf(CosmosSaveActor.Props(cdb));            
            act.Tell(new CosmosSaveActor.SaveRequest(null, null), TestActor);
            ExpectMsg<CosmosSaveActor.BadRequest>(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void BadCollection()
        {
            var cdb = new CosmosDB();
            var act = ActorOf(CosmosSaveActor.Props(cdb));
            act.Tell(new CosmosSaveActor.SaveRequest(new { id = 1 }, null), TestActor);
            ExpectMsg<CosmosSaveActor.BadRequest>(TimeSpan.FromSeconds(10));
        }

        [Test]
        public void GoodSave()
        {
            var cdb = new CosmosDB();            
            // clear out test document
            cdb.DeleteDocument("system", "1", "1");
            
            // do test
            var act = ActorOf(CosmosSaveActor.Props(cdb));
            act.Tell(new CosmosSaveActor.SaveRequest(new { id = "1", name = "test" }, "system"), TestActor);
            ExpectNoMsg(TimeSpan.FromSeconds(10));

            // check the DB
            var vals = cdb
                .GetDocumentQuery("system", "select * from c where c.id = '1'")
                .ToList();
            Assert.IsTrue(vals.Any());            
        }
    }
}
