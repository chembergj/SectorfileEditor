using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SectorfileEditor.Model
{
    public class SectorFileGeoLine : SectorFileLine
    {
        readonly Regex regex = new Regex(@"([N|S]\d\d\d?\.\d\d\.\d\d\.\d\d\d)\s+([E|W]\d\d\d?\.\d\d\.\d\d\.\d\d\d)");

        public string Start => regex.IsMatch(Data) ? regex.Matches(Data)[0].Value : null;
        public string End => regex.IsMatch(Data) ? regex.Matches(Data)[1].Value : null;
        
        public string ColorName => Data.Length > 59 ? Data.Substring(60) : "";
    }
}
