using Akka.Actor;
using DATC_Receiver.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DATC_Receiver.Actors
{
    class ICAOLookupActor : ReceiveActor
    {
        private Dictionary<string, ICAOData> datas = new Dictionary<string, ICAOData>();
        private Dictionary<string, ICAOAircraft> aircraft = new Dictionary<string, ICAOAircraft>();

        public ICAOLookupActor()
        {
            Receive<AircraftRequest>(r =>
            {
                if (r.Craft != null && aircraft.ContainsKey(r.Craft.ToUpper()))
                    Sender.Tell(new AircraftResponse()
                    {
                        ICAOAircraft = aircraft[r.Craft.ToUpper()],
                        Craft = r.Craft
                    });
                else
                    Sender.Tell(new AircraftResponse()
                    {
                        ICAOAircraft = null,
                        Craft = r.Craft
                    });
            });

            Receive<HexRequest>(r =>
            {
                if (r.Hex != null && datas.ContainsKey(r.Hex.ToUpper()))
                    Sender.Tell(new HexResponse()
                    {
                        ICAOData = datas[r.Hex.ToUpper()],
                        Hex = r.Hex
                    });
                else
                    Sender.Tell(new HexResponse()
                    {
                        ICAOData = null,
                        Hex = r.Hex
                    });
            });
        }

        public ICAOData GetData(string hex)
        {
            if (datas.ContainsKey(hex.ToUpper()))
                return datas[hex.ToUpper()];

            return null;
        }

        protected override void PreStart()
        {
            base.PreStart();

            if (File.Exists("icao.json"))
            {
                Console.WriteLine("ICAO exists");
                aircraft = JsonConvert.DeserializeObject<Dictionary<string, ICAOAircraft>>(File.ReadAllText("icao.json"));
            }
            else
            {
                Console.WriteLine("Creating ICAO");
                WebClient wc = new WebClient();
                var data = wc.DownloadString("http://192.168.1.148/dump1090-fa/db/aircraft_types/icao_aircraft_types.json");
                var vals = JsonConvert.DeserializeObject<dynamic>(data);
                foreach (var v in vals)
                {
                    string o = v.ToString();
                    var colon = o.IndexOf(":");
                    var id = o.Substring(1, o.LastIndexOf("\"", colon) - 1);
                    var icoa = JsonConvert.DeserializeObject<ICAOAircraft>(o.Substring(id.Length + 3));

                    try
                    {
                        aircraft.Add(id, icoa);
                    }
                    catch (Exception exAdd)
                    {
                        var e = exAdd.Message;
                    }
                }
                File.WriteAllText("icao.json", JsonConvert.SerializeObject(aircraft));
            }

            if (File.Exists("info.json"))
            {
                Console.WriteLine("INFO exists");
                datas = JsonConvert.DeserializeObject<Dictionary<string, ICAOData>>(File.ReadAllText("info.json"));
            }
            else
            {
                Console.WriteLine("creating INFO");
                var files = new List<string>() { "A", "C", "4", "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD" };
                var wc = new WebClient();
                foreach (var f in files)
                {
                    processFile(wc, f);
                }

                for (var i = 0; i < 220; i++)
                {
                    processFile(wc, $"A{i.ToString("X2")}");
                }

                File.WriteAllText("info.json", JsonConvert.SerializeObject(datas));
            }
            var z = "";
        }

        private void processFile(WebClient wc, string f)
        {
            try
            {
                var str = wc.DownloadString($"http://192.168.1.148/dump1090-fa/db/{f}.json");
                var vals = JsonConvert.DeserializeObject<dynamic>(str);

                foreach (var v in vals)
                {
                    string o = v.ToString();
                    if (!o.Contains("hildren"))
                    {
                        var colon = o.IndexOf(":");
                        var id = o.Substring(1, o.LastIndexOf("\"", colon) - 1);
                        var icoa = JsonConvert.DeserializeObject<ICAOData>(o.Substring(id.Length + 3));
                        if (f.Length > 1)
                            id = f + id;
                        try
                        {
                            datas.Add(id, icoa);
                        }
                        catch (Exception exAdd)
                        {
                            var e = exAdd.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ex.Message;
            }
        }

        #region Messages
        internal class AircraftRequest
        {
            public AircraftRequest(string craft)
            {
                Craft = craft;
            }
            public string Craft { get; private set; }
        }

        internal class AircraftResponse
        {
            public ICAOAircraft ICAOAircraft { get; set; }
            public string Craft { get; set; }
        }

        internal class HexRequest
        {
            public HexRequest(string hex)
            {
                Hex = hex;
            }
            public string Hex { get; private set; }
        }

        internal class HexResponse
        {
            public ICAOData ICAOData { get; set; }
            public string Hex { get; set; }
        }

        #endregion
    }
}