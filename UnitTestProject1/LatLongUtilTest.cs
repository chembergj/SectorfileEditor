﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SectorfileEditor.Model;
using static SectorfileEditor.Model.LatLongUtil;

namespace UnitTestProject1
{
    [TestClass]
    public class LatLongUtilTest
    {
        [TestMethod]
        public void TestYToLatIsInverseOfLatToY()
        {
            Assert.AreEqual(20.0, LatLongUtil.LatitudeToY(LatLongUtil.YToLatitude(20)), 0.0001);
        }

        // 
        [TestMethod]
        public void TestGetCoordinate()
        {
            var coord = LatLongUtil.GetCoordinate(91.5);
            Assert.AreEqual(new Coordinate() { Degree = 91, Minutes = 30, Seconds = 0, Mseconds = 0 }, coord);
        }

        [TestMethod]
        public void TestConvertDegreeAngleToDouble()
        {
            var coord1 = LatLongUtil.ConvertDegreeAngleToDouble("N055.37.40.078");
            Assert.AreEqual(55, (int)coord1);

            var coord2 = LatLongUtil.ConvertDegreeAngleToDouble("N055.40.40.078");
            Assert.AreEqual(55, (int)coord2);
            Assert.IsTrue(coord1 < coord2);
        }

        [TestMethod]
        public void TestLatitudeToY()
        {
            var y = LatLongUtil.LatitudeToY(55.6);
           
        }

        [TestMethod]
        public void TestGetPointFromLatLongString()
        {
            LatLongUtil.ScaleTransform.ScaleX = 100000;
            LatLongUtil.ScaleTransform.ScaleY = 100000;
            

            LatLongUtil.TranslateTransform.X = -12.65;
            LatLongUtil.TranslateTransform.Y = -67.239;

            // LatLongUtil.ScaleTransform.CenterX = 1.00; ; //  LatLongUtil.TranslateTransform.X;
            // LatLongUtil.ScaleTransform.CenterY = 80.239; //LatLongUtil.TranslateTransform.Y;

            var point = LatLongUtil.GetPointFromLatLongString("N055.37.40.078 E012.39.12.954");
            var str = LatLongUtil.GetLatLongStringFromPoint(point);
            Assert.AreEqual("N055.37.40.078 E012.39.12.953", str);  // 1 msec off, probably due to rounding errors

            var point2 = LatLongUtil.GetPointFromLatLongString("N55.37.40.078 E12.39.12.954");
            Assert.AreEqual(point.X, point2.X);
            Assert.AreEqual(point.Y, point2.Y);
        }
    }
}