using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SectorfileEditor;
using SectorfileEditor.Control;

namespace UnitTestProject1
{
    [TestClass]
    public class SctFileReaderTest
    {
        [TestMethod]
        public void TestReading()
        {
            int geoLineCount = 0;
            int defineCount = 0;
            int regionCount = 0;

            var reader = new SctFileReader();
            reader.GeoLineHandler = 
                line =>
                {
                    geoLineCount++;
                    Assert.IsFalse(String.IsNullOrWhiteSpace(line.Data));
                    if (geoLineCount == 1)
                    {
                        Assert.AreEqual("N062.59.56.504 E008.19.38.824 N062.59.42.774 E008.19.15.589", line.Data);
                        Assert.AreEqual("N062.59.56.504 E008.19.38.824", line.Start);
                        Assert.AreEqual("N062.59.42.774 E008.19.15.589", line.End);
                        Assert.IsTrue(line.ColorName.Length == 0);
                    }
                    else if(geoLineCount == 36050)
                    {
                        Assert.AreEqual("N056.50.52.551 E009.27.53.104 N056.50.53.237 E009.27.53.612 BUILDING", line.Data);
                        Assert.AreEqual("Hangar medium", line.Comment);
                        Assert.AreEqual("N056.50.52.551 E009.27.53.104", line.Start);
                        Assert.AreEqual("N056.50.53.237 E009.27.53.612", line.End);
                        Assert.AreEqual("BUILDING", line.ColorName);
                    }
                };
            reader.DefineHandler = (define,color) =>
            {
                if(defineCount == 0)
                {
                    Assert.AreEqual("ISLAND", define);
                    Assert.AreEqual(32768, color);
                }
                else if(defineCount == 20)
                {
                    Assert.AreEqual("BASE", define);
                    Assert.AreEqual(7290880, color);
                }

                defineCount++;
            };
            reader.RegionHandler = region =>
            {
                if(regionCount == 0)
                {
                    Assert.AreEqual("BASECPH", region.Name);
                    Assert.AreEqual("BASE", region.ColorName);
                    Assert.AreEqual(7, region.Coordinates.Count);
                    Assert.AreEqual("N055.43.56.000 E012.48.34.000", region.Coordinates[0]);
                    Assert.AreEqual("N055.41.58.000 E012.25.56.000", region.Coordinates[5]);
                    Assert.AreEqual("N055.43.56.000 E012.48.34.000", region.Coordinates[6]);
                }
                else if(regionCount == 342)
                {
                    Assert.AreEqual("EKRD", region.Name);
                    Assert.AreEqual("RWY", region.ColorName);
                    Assert.AreEqual(4, region.Coordinates.Count);
                    Assert.AreEqual("N056.30.19.553 E010.01.45.910", region.Coordinates[0]);
                    Assert.AreEqual("N056.30.28.433 E010.02.36.064", region.Coordinates[2]);
                    Assert.AreEqual("N056.30.20.360 E010.01.45.387", region.Coordinates[3]);
                }
                
                regionCount++;
            };
            reader.Parse("..\\..\\Testdata\\EKDK_official_16_13.sct");
            Assert.AreEqual(36050, geoLineCount);
            Assert.AreEqual(343, regionCount);
        }
    }
}
