using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model.SectorFile
{
    public class SectorFileInfo
    {
        public string Name { get; set; }
        public string DefaultCallsign { get; set; }
        public string DefaultAirport { get; set; }
        public string DefaultLongitude { get; set; }
        public string DefaultLatitude { get; set; }
        public double NMPerLatDegree { get; set; }
        public double NMPerLongDegree { get; set; }
        public double MagVar { get; set; }
    }
}
