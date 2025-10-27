using UnityEngine;

    public class GpsSample : MonoBehaviour
    {
        public GPS gps;

        void Update()
        {
            // ��: �����û
            double targetLat = 37.5662952, targetLon = 126.9779451;
            double meters = gps.DistanceTo(targetLat, targetLon, GPS.DistUnit.meter);
            double bearing = GPS.InitialBearing(gps.Latitude, gps.Longitude, targetLat, targetLon);
            // Debug.Log($"�Ÿ� {meters:F1} m / ���� {bearing:F1}��");
        }
    }
