using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model
{
    public class SectorFileLatLongRegion
    {
        private List<SectorFileLatLongDegreePoint> coordinates = new List<SectorFileLatLongDegreePoint>();

        public string Name { get; protected set; }
        public string ColorName { get; protected set; }
        public List<SectorFileLatLongDegreePoint> Coordinates { get { return coordinates; } }

        public SectorFileLatLongRegion(string name, string colorName, List<SectorFileLatLongDegreePoint> coordinates)
        {
            Name = name;
            ColorName = colorName;
            this.coordinates.AddRange(coordinates);
        }
    }
}
