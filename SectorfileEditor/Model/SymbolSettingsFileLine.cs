using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Model
{
    public class SymbolSettingsFileLine: SettingsFileLine
    {
        public long RGB {  get { return long.Parse(Values[0]);  } }
        public byte R { get { return (byte)(RGB & 0xff); } }
        public byte G { get { return (byte)((RGB & 0xff00) >> 8); } }
        public byte B { get { return (byte)((RGB & 0xff0000) >> 16); } }

        public double SymbolSize { get { return double.Parse(Values[1]); } }      
    }
}
