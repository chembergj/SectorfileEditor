using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model
{
    public class LatLongRegion
    {
        private List<LatLongDegreePoint> coordinates = new List<LatLongDegreePoint>();

        public string Name { get; protected set; }
        public string ColorName { get; protected set; }
        public List<LatLongDegreePoint> Coordinates { get { return coordinates; } }

        public LatLongRegion(string name, string colorName, List<LatLongDegreePoint> coordinates)
        {
            Name = name;
            ColorName = colorName;
            this.coordinates.AddRange(coordinates);
        }
    }
}
