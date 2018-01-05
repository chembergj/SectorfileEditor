using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SectorfileEditor.Model
{
    public class LatLongDegreeLine
    {
        public LatLongDegreeLine(LatLongDegreePoint start, LatLongDegreePoint end, string color)
        {
            Start = start;
            End = end;
            Color = color;
        }

        public LatLongDegreePoint Start { get; protected set; }
        public LatLongDegreePoint End { get; protected set; }
       
        public string Color { get; protected set; }
    }
}
