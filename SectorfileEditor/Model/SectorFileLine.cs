using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model
{
    public abstract class SectorFileLine
    {
        public string Data { get; set; }
        public string Comment { get; set; }
    }
}
