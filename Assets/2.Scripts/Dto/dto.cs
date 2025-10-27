using System;
using System.Collections.Generic;

namespace Data
{
    public abstract class MissonDto
    {

    }

    [System.Serializable]
    public struct GeoCoordinate
    {
        public double Latitude;
        public double Longitude;

        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}