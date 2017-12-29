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
        public Action<string, long> DefineHandler { get; set; }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Parse(string filename)
        {
            logger.Info("Parsing file " + filename);

            string nextline = "";
            reader = new StreamReader(filename);
            do
            {
                if(nextline.StartsWith("[GEO]"))
                {
                    nextline = ReadGeoSection(reader);
                }
                else if(nextline.StartsWith("#define"))
                {
                    nextline = ReadDefine(reader, nextline);
                }
                else
                {
                    nextline = reader.ReadLine();
                }
                
            } while (!reader.EndOfStream);
            
        }

        // Example: #define ISLAND 32768
        private string ReadDefine(StreamReader reader, string defineLine)
        {
            var splittedDefine = defineLine.Split(new char[]{ ' '}, StringSplitOptions.RemoveEmptyEntries);
            if (splittedDefine.Count() == 3)
            { 
                DefineHandler(splittedDefine[1], long.Parse(splittedDefine[2]));
            }
            else
            {
                logger.Debug("Ignoring line due to unexpected format: " + defineLine);
            }
            return reader.ReadLine();
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
