using Akka.TestKit.NUnit3;
using DATC_Receiver.Actors;
using DATC_Receiver.DataStructures;
using DATC_Receiver.Helpers;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DATC_Receiver.Tests
{
    [TestFixture]
    class FlightActorTest : TestKit
    {
        [Test]
        public void StartUpActor()
        {
            var cdb = new CosmosDB();
            var save = ActorOf(CosmosSaveActor.Props(cdb));
            var icao = ActorOf<ICAOLookupActor>();
            var a = ActorOf(FlightActor.Props());
            a.Tell(new FlightActor.FlightActorInit(save, "123", icao), TestActor);
            ExpectNoMsg(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void SubmitNullFlightData()
        {
            // resources for actor
            var cdb = new CosmosDB();
            cdb.OpenConnection();
            var save = ActorOf(CosmosSaveActor.Props(cdb));
            var icao = ActorOf<ICAOLookupActor>();

            var a = ActorOf(FlightActor.Props());
            a.Tell(new FlightActor.FlightActorInit(save, "TEST", icao), TestActor);
            Watch(a);
            a.Tell(new FlightActor.FlightDataRequest()
            {
                deviceId = "TEST",
                flightData = null,
                now = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            }, TestActor);

            ExpectTerminated(a, TimeSpan.FromSeconds(5));

        }

        [Test]
        public  void SubmitOne()
        {
            // resources for actor
            var cdb = new CosmosDB();
            cdb.OpenConnection();
            var save = ActorOf(CosmosSaveActor.Props(cdb));
            var icao = ActorOf<ICAOLookupActor>();            

            // test data
            var data = getTestData();
            var info = data[0];
            var tf = info.aircraft.First(z => z.flight != null);            
            var str = "activeSnap:" + tf.flight.Trim();

            var a = ActorOf(FlightActor.Props());
            a.Tell(new FlightActor.FlightActorInit(save, tf.flight, icao), TestActor);

            a.Tell(new FlightActor.FlightDataRequest()
            {
                deviceId = info.deviceId,
                flightData = tf,
                now = info.now
            }, TestActor);

            ExpectNoMsg(TimeSpan.FromSeconds(5));

            var res = cdb.GetDocumentQuery<FlightDataSnapshot>("flights")
                .Where(z => z.id == str)
                .ToList();

            Assert.Greater(res.Count, 0);

            Assert.AreEqual(res[0].now, info.now);
            Assert.AreEqual(res[0].lat, tf.lat);
        }

        [Test]
        public void SubmitMany()
        {
            // resources for actor
            var cdb = new CosmosDB();

            cdb.OpenConnection();
            var save = ActorOf(CosmosSaveActor.Props(cdb));
            var icao = ActorOf<ICAOLookupActor>();

            // test data
            var data = getTestData();

            DeviceReading dr = data[0];
            FlightData fd = dr.aircraft.First(z =>  z.flight != null && z.flight.Trim() == "JBU238");

            var flight = fd.flight.Trim();
            var str = "activeSnap:" + flight.Trim();

            var a = ActorOf(FlightActor.Props());
            a.Tell(new FlightActor.FlightActorInit(save, flight.Trim(), icao), TestActor);

            System.Threading.Thread.Sleep(1000);

            data = data.OrderBy(z => z.now).ToList();

            foreach (var d in data.Take(2))
            {                                
                if (d.aircraft.Any(z => z.flight != null && z.flight.Trim() == flight))
                {
                    fd = d.aircraft.First(z => z.flight != null && z.flight.Trim() == flight);
                    dr = d;
                    a.Tell(new FlightActor.FlightDataRequest()
                    {
                        deviceId = d.deviceId,
                        flightData = fd,
                        now = d.now
                    }, TestActor);
                }
            }

            ExpectNoMsg(TimeSpan.FromSeconds(5));

            var res = cdb.GetDocumentQuery<FlightDataSnapshot>("flights")
                .Where(z => z.id == str && z.flight == flight.Trim())
                .OrderByDescending(z => z.now)
                .ToList();

            Assert.Greater(res.Count, 0);

            var m = data.Where(z => z.now == res[0].now);

            Assert.AreEqual(res.First().now, dr.now);
            Assert.AreEqual(res.First().lat, fd.lat);
        }

        [Test]
        public void LoadTestData()
        {
            var data = getTestData();
            Assert.Greater(data.Count, 0);
            var info = data[0];
            Assert.IsNotNull(info.aircraft);
            Assert.Greater(info.aircraft.Count, 0);
            var craft = info.aircraft.First(z => z.flight != null);
            Assert.IsNotNull(craft.flight);
            Assert.IsNotNull(craft.hex);
        }

        class TestDataWrapper
        {
            public DeviceReading ToSend { get; set; }
        }

        List<DeviceReading> getTestData()
        {
            var ret = new List<DeviceReading>();
            foreach (var f in Directory.GetFiles("TestData"))
            {
                var data = File.ReadAllText(f);
                var reading = (JsonConvert.DeserializeObject<TestDataWrapper>(data)).ToSend;                
                ret.Add(reading);
            }

            return ret;
        }

    }
}
