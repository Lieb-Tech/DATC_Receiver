using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.DataStructures
{
    public class DeviceReading
    {
        public string deviceId { get; set; }
        public long idx { get; set; }
        public double now { get; set; }
        public long messages { get; set; }
        public List<FlightData> aircraft { get; set; }
    }
}
