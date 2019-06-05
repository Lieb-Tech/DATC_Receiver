using Akka.TestKit.NUnit3;
using DATC_Receiver.Actors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DATC_Receiver.Tests
{
    [TestFixture]
    class ICAOLookupActorTest : TestKit
    {
        [Test]
        public void NoCacheFiles()
        {
            if (File.Exists("icao.json"))
                File.Delete("icao.json");

            if (File.Exists("info.json"))
                File.Delete("info.json");

            Assert.DoesNotThrow(() =>
            {
                var actor = ActorOf<ICAOLookupActor>();
                actor.Tell(new ICAOLookupActor.AircraftRequest("A321"), TestActor);
                ExpectMsg<ICAOLookupActor.AircraftResponse>(TimeSpan.FromMinutes(5));

                Assert.IsTrue(File.Exists("icao.json"));
                Assert.IsTrue(File.Exists("info.json"));
            });
        }

        [Test]
        public void A321Lookup()
        {
            var actor = ActorOf<ICAOLookupActor>();
            actor.Tell(new ICAOLookupActor.AircraftRequest("A321"), TestActor);
            var resp = ExpectMsg<ICAOLookupActor.AircraftResponse>(TimeSpan.FromMinutes(1));
            Assert.AreEqual("L2J", resp.ICAOAircraft.desc);
            Assert.AreEqual("M", resp.ICAOAircraft.wtc);
        }
        [Test]
        public void BadCraftLookup()
        {
            var actor = ActorOf<ICAOLookupActor>();
            actor.Tell(new ICAOLookupActor.AircraftRequest("A"), TestActor);
            var resp = ExpectMsg<ICAOLookupActor.AircraftResponse>(TimeSpan.FromMinutes(1));
            Assert.IsNull(resp.ICAOAircraft);
        }
        [Test]
        public void NullCraftLookup()
        {
            var actor = ActorOf<ICAOLookupActor>();
            actor.Tell(new ICAOLookupActor.AircraftRequest(null), TestActor);
            var resp = ExpectMsg<ICAOLookupActor.AircraftResponse>(TimeSpan.FromMinutes(1));
            Assert.IsNull(resp.ICAOAircraft);            
        }

        [Test]
        public void HexLookup()
        {
            var actor = ActorOf<ICAOLookupActor>();
            actor.Tell(new ICAOLookupActor.HexRequest("E010D"), TestActor);
            var resp = ExpectMsg<ICAOLookupActor.HexResponse>(TimeSpan.FromMinutes(1));
            Assert.IsNull(resp.ICAOData.desc);
            Assert.AreEqual("GLF5", resp.ICAOData.t);
            Assert.AreEqual("97-0400", resp.ICAOData.r);
        }
        [Test]
        public void BadHexLookup()
        {
            var actor = ActorOf<ICAOLookupActor>();
            actor.Tell(new ICAOLookupActor.HexRequest("E"), TestActor);
            var resp = ExpectMsg<ICAOLookupActor.HexResponse>(TimeSpan.FromMinutes(1));
            Assert.IsNull(resp.ICAOData);
        }
        [Test]
        public void NullHexLookup()
        {
            var actor = ActorOf<ICAOLookupActor>();
            actor.Tell(new ICAOLookupActor.HexRequest(null), TestActor);
            var resp = ExpectMsg<ICAOLookupActor.HexResponse>(TimeSpan.FromMinutes(1));
            Assert.IsNull(resp.ICAOData);            
        }
    }
}
