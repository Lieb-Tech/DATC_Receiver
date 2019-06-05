using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.DataStructures
{
    public class FlightData
    {
        public string hex { get; set; }
        public long alt_baro { get; set; }
        public int version { get; set; }
        public int nac_p { get; set; }
        public int nac_v { get; set; }
        public int sil { get; set; }
        public string sil_type { get; set; }
        public int sda { get; set; }
        public List<object> mlat { get; set; }
        public List<object> tisb { get; set; }
        public int messages { get; set; }
        public double seen { get; set; }
        public double rssi { get; set; }
        public int? alt_geom { get; set; }
        public double? gs { get; set; }
        public double? track { get; set; }
        public int? baro_rate { get; set; }
        public string flight { get; set; }
        public string squawk { get; set; }
        public string emergency { get; set; }
        public string category { get; set; }
        public double? lat { get; set; }
        public double? lon { get; set; }
        public int? nic { get; set; }
        public int? rc { get; set; }
        public double? seen_pos { get; set; }
        public int? nic_baro { get; set; }
        public int? gva { get; set; }
        public double? nav_qnh { get; set; }
        public int? nav_altitude { get; set; }
        public double? nav_heading { get; set; }
        public List<string> nav_modes { get; set; }
        public int? geom_rate { get; set; }
    }
}
