namespace SectorfileEditor.Model
{
    public class LatLongDegreePoint
    {
        public LatLongDegreePoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Present X/Y representation, will change as LatLongUtil tranforms matrices change
        public double X { get; set; }
        public double Y { get; set; }
    }
}
