using System;
using System.Collections.Generic;
using System.Text;

namespace DATC_Receiver.DataStructures
{
    public class FlightDataExtended : FlightData
    {
        public FlightDataExtended(FlightData flightData)
        {
            base.alt_baro = flightData.alt_baro;
            base.alt_geom = flightData.alt_geom;
            base.baro_rate = flightData.baro_rate;
            base.category = flightData.category;
            base.emergency = flightData.emergency;
            base.flight = flightData.flight;
            base.geom_rate = flightData.geom_rate;
            base.gs = flightData.gs;
            base.gva = flightData.gva;
            base.hex = flightData.hex;
            base.lat = flightData.lat;
            base.lon = flightData.lon;
            base.messages = flightData.messages;
            base.mlat = flightData.mlat;
            base.nac_p = flightData.nac_p;
            base.nac_v = flightData.nac_v;
            base.nav_altitude = flightData.nav_altitude;
            base.nav_heading = flightData.nav_heading;
            base.nav_modes = flightData.nav_modes;
            base.nav_qnh = flightData.nav_qnh;
            base.nic = flightData.nic;
            base.nic_baro = flightData.nic_baro;
            base.rc = flightData.rc;
            base.rssi = flightData.rssi;
            base.sda = flightData.sda;
            base.seen = flightData.seen;
            base.seen_pos = flightData.seen_pos;
            base.sil = flightData.sil;
            base.sil_type = flightData.sil_type;
            base.squawk = flightData.squawk;
            base.tisb = flightData.tisb;
            base.track = flightData.track;
            base.version = flightData.version;

            id = $"flight:{flight}:{hex}:{seen}";
        }

        public string id { get; set; }
        public ICAOData icoaData { get; set; }
        public ICAOAircraft icoaAircraft { get; set; }

        public double spdDelta { get; set; }
        public double altDelta { get; set; }
    }
}
