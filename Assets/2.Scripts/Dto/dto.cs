using System;
using System.Collections.Generic;

namespace Data
{
    public abstract class MissonDto
    {

    }

    public class SearchPathCoordinate
    {
        public List<GeoCoordinate> pathCoordinates; // 길찾기 경로 좌표 리스트
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