using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.DataStructures
{
    class FlightDataSnapshotArchive
    {
        public List<FlightDataSnapshot> archive = new List<FlightDataSnapshot>();

        public ICAOAircraft icaoAircraft { get; set; }
        public ICAOData icaoData { get; set; }

        public string deviceId { get; set; }

        public string flightCode { get; set; }
        public string hex { get; set; }

    }
}
