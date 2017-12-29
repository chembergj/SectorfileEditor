using NLog;
using SectorfileEditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorfileEditor.Control
{
    public class SettingsFileReader<T> where T: SettingsFileLine, new()
    {
        public enum KeyIndex
        {
            GeoLine = 0,
            SectorInactiveBackground = 1
        };

        private StreamReader reader;
        public readonly List<string> knownKeys = new List<string>() { "Geo:line", "Sector:inactive sector background" };

        public Action<T> SettingsLineHandler { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Parse(string filename)
        {
            logger.Info("Reading file " + filename);

            string nextline = "";
            reader = new StreamReader(filename);
            do
            {
                nextline = reader.ReadLine();
                var foundKey = knownKeys.Find(key => nextline.StartsWith(key));
                if(foundKey != null)
                {
                    var line = new T() { Key = foundKey };
                    line.Values = nextline.Remove(0, foundKey.Length + 1).Split(':').ToList();
                    SettingsLineHandler(line);
                }
                               
            } while (!reader.EndOfStream);
            
        }
        
    }
}
