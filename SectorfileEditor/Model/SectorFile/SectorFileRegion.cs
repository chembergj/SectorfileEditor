using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model.SectorFile
{
    public class SectorFileRegion
    {
        private List<string> coordinates = new List<string>();

        public string Name { get; protected set; }
        public string ColorName { get; protected set; }
        public List<string> Coordinates { get { return coordinates; } }

        public SectorFileRegion(string name, string colorName)
        {
            Name = name;
            ColorName = colorName;
        }
    }
}
