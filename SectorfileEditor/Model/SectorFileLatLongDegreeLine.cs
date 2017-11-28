using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SectorfileEditor.Model
{
    public class SectorFileLatLongDegreeLine
    {
        public SectorFileLatLongDegreeLine(double latitudeStart, double longitudeStart, double latitudeEnd, double longitudeEnd)
        {
            LatitudeStart = latitudeStart;
            LatitudeEnd = latitudeEnd;
            LongitudeStart = longitudeStart;
            LongitudeEnd = longitudeEnd;
        }

        public double LatitudeStart { get; protected set; }
        public double LatitudeEnd { get; protected set; }
        public double LongitudeStart { get; protected set; }
        public double LongitudeEnd { get; protected set; }
    }
}
