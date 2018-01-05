using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace SectorfileEditor.Model
{
    public class LatLongUtil
    {
        public static readonly Regex RegexCoordinate = new Regex(@"^([N|S]\d\d\d?\.\d\d\.\d\d\.\d\d\d)\s+([E|W]\d\d\d?\.\d\d\.\d\d\.\d\d\d)");

        public static ScaleTransform ScaleTransform { get; set; }
        public static TranslateTransform TranslateTransform { get; set; }
        public const double multFactor = 100000;
        public const double xoffset = -8;
        public const double yoffset = -81;

        static LatLongUtil() {
            ScaleTransform = new ScaleTransform();
            TranslateTransform = new TranslateTransform();
        }

        public static double ConvertDegreeAngleToDouble(string point)
        {
            //Example: 17.21.18S

            var multiplier = (point.Contains("S") || point.Contains("W")) ? -1 : 1; //handle south and west

            point = Regex.Replace(point, "[^0-9.]", ""); //remove the characters

            var pointArray = point.Split('.'); //split the string.

            //Decimal degrees = 
            //   whole number of degrees, 
            //   plus minutes divided by 60, 
            //   plus seconds divided by 3600

            var degrees = Double.Parse(pointArray[0]);
            var minutes = Double.Parse(pointArray[1]) / 60;
            var seconds = Double.Parse(pointArray[2] + "," + pointArray[3]) / 3600;

            return degrees + minutes + seconds;
        }

        public static double YToLatitude(double y)
        {
            return System.Math.Atan(System.Math.Exp(
                y / 180 * System.Math.PI
            )) / System.Math.PI * 360 - 90;
        }
        public static double LatitudeToY(double latitude)
        {
            return System.Math.Log(System.Math.Tan(
                (latitude + 90) / 360 * System.Math.PI
            )) / System.Math.PI * 180;
        }

        public static Point GetPointFromLatLongString(string latLong)
        {
            var splitted = latLong.Trim().Split(' ');
            var latdec = ConvertDegreeAngleToDouble(splitted[0]);
            var londec = ConvertDegreeAngleToDouble(splitted[1]);

            return Transform(latdec, londec);
        }

        public static Point Transform(double latdec, double londec)
        {
            return ScaleTransform.Transform(TranslateTransform.Transform(new Point(londec, -LatitudeToY(latdec))));
        }

        public static Point GetLatLongDecimalPointFromLatLongString(string latLong)
        {
            var splitted = latLong.Trim().Split(' ');
            var latdec = ConvertDegreeAngleToDouble(splitted[0]);
            var londec = ConvertDegreeAngleToDouble(splitted[1]);

            return new Point(latdec, londec);
        }

        public struct Coordinate
        {
            public int Degree;
            public int Minutes;
            public int Seconds;
            public int Mseconds;
        }

        public static void GetLatLongDegreeFromPoint(Point p, out double latitude, out double longitude)
        {
            var transformedPoint = TranslateTransform.Inverse.Transform(ScaleTransform.Inverse.Transform(p));
            latitude = YToLatitude(-transformedPoint.Y);
            longitude = transformedPoint.X;
        }
        public static string GetLatLongStringFromPoint(Point p)
        {
            double lat;
            double lon;

            GetLatLongDegreeFromPoint(p, out lat, out lon);
            var transformedPoint = TranslateTransform.Inverse.Transform(ScaleTransform.Inverse.Transform(p));

            // Latitude
            while (lat < -180.0)
                lat += 360.0;

            while (lat > 180.0)
                lat -= 360.0;

            bool latIsNegative = lat < 0;
            lat = Math.Abs(lat);

            var latitude = GetCoordinate(lat);

            // Longitude

            while (lon < -180.0)
                lon += 360.0;

            while (lon > 180.0)
                lon -= 360.0;

            bool lonIsNegative = lon < 0;
      
            var longitude = GetCoordinate(lon);

            return string.Format("{0}{1:000}.{2:00}.{3:00}.{4:000} {5}{6:000}.{7:00}.{8:00}.{9:000}",
                latIsNegative ? 'S' : 'N', latitude.Degree, latitude.Minutes, latitude.Seconds, latitude.Mseconds,
                lonIsNegative ? 'W' : 'E', longitude.Degree, longitude.Minutes, longitude.Seconds, longitude.Mseconds);

        }

        public static Coordinate GetCoordinate(double decDegrees)
        {
            Coordinate coordinate;

            coordinate.Degree = (int)Math.Floor(decDegrees);

            var delta = decDegrees - coordinate.Degree;
            var numSeconds = (int)Math.Floor(3600.0 * delta);
            coordinate.Seconds = numSeconds % 60;
            coordinate.Minutes = (int)Math.Floor(numSeconds / 60.0);
            delta = delta * 3600.0 - numSeconds;

            coordinate.Mseconds = (int)(1000.0 * delta);

            return coordinate;
        }
    }
}
