using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SectorfileEditor.Model.SectorFile;

namespace UnitTestProject1.Model
{
    [TestClass]
    public class SectorFileGeoLineTest
    {
        [TestMethod]
        public void TestWithPreedingZeros()
        {
            var point = new SectorFileGeoLine() { Data = "N055.37.26.666 E012.39.34.725 N055.37.27.699 E012.39.35.305 TWY-LINE ; NEW ISLAND F STANDS EAST" };
            Assert.AreEqual("N055.37.26.666 E012.39.34.725", point.Start);
            Assert.AreEqual("N055.37.27.699 E012.39.35.305", point.End);
        }

        [TestMethod]
        public void TestWithoutPreedingZeros()
        {
            var point = new SectorFileGeoLine() { Data = "N55.37.26.666 E12.39.34.725 N55.37.27.699 E12.39.35.305 TWY-LINE ; NEW ISLAND F STANDS EAST" };
            Assert.AreEqual("N55.37.26.666 E12.39.34.725", point.Start);
            Assert.AreEqual("N55.37.27.699 E12.39.35.305", point.End);
        }
    }
}
