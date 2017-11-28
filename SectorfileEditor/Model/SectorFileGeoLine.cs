using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model
{
    public class SectorFileGeoLine: SectorFileLine
    {
        public string Start => Data.Substring(0, 29);

        public string End => Data.Substring(30, 29);

        public string ColorName => Data.Length > 59 ? Data.Substring(60) : "";
    }
}
