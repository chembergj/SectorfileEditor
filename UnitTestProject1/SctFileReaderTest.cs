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

            reader.Parse("..\\..\\Testdata\\EKDK_official_16_13.sct");
            Assert.AreEqual(36050, geoLineCount);

        }
    }
}
