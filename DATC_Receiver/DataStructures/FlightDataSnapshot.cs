using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.DataStructures
{
    public class FlightDataSnapshot
    {        
        public string flight { get; set; }  // partion key
        public string id { get; set; }
        public double now { get; set; }
        public long alt { get; set; }
        public double track { get; set; }
        public double spd { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public double spdDelta { get; set; }
        public double altDelta { get; set; }
    }
}