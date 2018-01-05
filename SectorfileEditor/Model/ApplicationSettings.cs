using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SectorfileEditor.Model
{
    public class ApplicationSettings
    {
        private static ApplicationSettings instance = null;

        // User settings
        public string Center { get; set; }
        public double ZoomFactor { get; set; }

        protected ApplicationSettings() { }

        public static ApplicationSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApplicationSettings();
                }

                return instance;
            }
        }

        public static void Save(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ApplicationSettings));
                xmls.Serialize(sw, instance);
            }
        }
        public static void Load(string filename)
        {
            using (StreamReader sw = new StreamReader(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ApplicationSettings));
                instance = xmls.Deserialize(sw) as ApplicationSettings;
            }
        }
    }
}
