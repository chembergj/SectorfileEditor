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
    public class SctFileReader
    {
        private StreamReader reader;

        public Action<SectorFileGeoLine> GeoLineHandler { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Parse(string filename)
        {
            logger.Debug("Parsing file " + filename);

            string nextline = "";
            reader = new StreamReader(filename);
            do
            {
                if(nextline.StartsWith("[GEO]"))
                {
                    nextline = ReadGeoSection(reader);
                }
                else
                {
                    nextline = reader.ReadLine();
                }
            } while (!reader.EndOfStream);
            
        }

        private bool IsCommentLine(string line)
        {
            return line.StartsWith(";") || line.StartsWith(":");
        }

        private string ReadGeoSection(StreamReader reader)
        {
            string line;
            do
            {
                line = reader.ReadLine();

                if(IsCommentLine(line))
                {
                    continue;
                }

                if (line.StartsWith("["))
                {
                    return line;
                }
                else if (!String.IsNullOrEmpty(line))
                {
                    var sectorFileLine = new SectorFileGeoLine();
                    var splittedLine = line.Split(';');
                    sectorFileLine.Data = splittedLine[0].Trim();
                    if(splittedLine.Length > 1)
                    {
                        sectorFileLine.Comment = splittedLine[1].Trim();
                    }
                    GeoLineHandler(sectorFileLine);
                }
            } while (!reader.EndOfStream);

            return "";
        }
    }
}
