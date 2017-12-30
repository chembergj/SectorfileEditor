namespace SectorfileEditor.Model
{
    public class SectorFileLatLongDegreePoint
    {
        public SectorFileLatLongDegreePoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }
    }
}
