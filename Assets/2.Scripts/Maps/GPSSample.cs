using UnityEngine;

    public class GpsSample : MonoBehaviour
    {
        public GPS gps;

        void Update()
        {
            // 예: 서울시청
            double targetLat = 37.5662952, targetLon = 126.9779451;
            double meters = gps.DistanceTo(targetLat, targetLon, GPS.DistUnit.meter);
            double bearing = GPS.InitialBearing(gps.Latitude, gps.Longitude, targetLat, targetLon);
            // Debug.Log($"거리 {meters:F1} m / 방위 {bearing:F1}°");
        }
    }
