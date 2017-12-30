using NLog;
using SectorfileEditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SectorfileEditor.Control
{
    public class SctFileReader
    {
        private StreamReader reader;

        public Action<SectorFileGeoLine> GeoLineHandler { get; set; }
        public Action<string, long> DefineHandler { get; set; }
        public Action<SectorFileRegion> RegionHandler { get; set; }

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
                else if (nextline.StartsWith("[REGIONS]"))
                {
                    nextline = ReadRegionsSection(reader);
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

        private string ReadRegionsSection(StreamReader reader)
        {
            string line = reader.ReadLine();
            do
            {
                if (IsCommentLine(line) || line.Length == 0)
                {
                    line = reader.ReadLine();
                }
                else if (line.StartsWith("["))
                {
                    return line;
                }
                else if (!String.IsNullOrEmpty(line))
                {
                    RegionHandler(ReadRegion(reader, line, out line));
                }
            } while (!reader.EndOfStream);

            return line;
        }

        private SectorFileRegion ReadRegion(StreamReader reader, string line, out string nextline)
        {
            var splitted = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var region = new SectorFileRegion(splitted[0], splitted[1]);
            string maybeCoordinate = (splitted[2] + " " + splitted[3]).Trim(' ', '\t');
            var matches = LatLongUtil.RegexCoordinate.Matches(maybeCoordinate);
            while(matches.Count == 1)
            {
                region.Coordinates.Add(matches[0].Value);
                maybeCoordinate = reader.ReadLine().Trim(' ', '\t');
                while ((IsCommentLine(maybeCoordinate) || maybeCoordinate.Length == 0) && !reader.EndOfStream)
                {
                    // Skip commentlines or empty lines
                    maybeCoordinate = reader.ReadLine().Trim(' ', '\t');
                }
                if (reader.EndOfStream)
                {
                    break;
                }
                else
                { 
                    matches = LatLongUtil.RegexCoordinate.Matches(maybeCoordinate);
                }
            }

            // A new region, or sct file section, has started
            nextline = maybeCoordinate;
            return region;
        }
    }
}
