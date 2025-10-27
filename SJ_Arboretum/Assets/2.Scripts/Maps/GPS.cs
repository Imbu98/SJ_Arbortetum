using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class GPS : MonoBehaviour
{
    // 현재 좌표(읽기용)
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    // 자동 시작 여부
    [SerializeField] private bool autoStart = true;

    IEnumerator Start()
    {
#if UNITY_ANDROID
        // 안드로이드 위치 권한 요청
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            Permission.RequestUserPermission(Permission.FineLocation);
#endif
        if (autoStart)
            yield return InitializeGPSServices();
    }

    IEnumerator InitializeGPSServices()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("GPS disabled by user");
            yield break;
        }

        // 정확도(distanceFilter 0.2m, 0.2m 마다 업데이트)
        Input.location.Start(0.2f, 0.2f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.LogWarning("GPS init timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location");
            yield break;
        }

        // 나침반(방위각) 필요 시 활성화 할 수 있음
        Input.compass.enabled = true;
    }

    void Update()
    {
        Latitude = 36.498028;
        Longitude = 127.285501;
        if (Input.location.status != LocationServiceStatus.Running) return;

        
        //var last = Input.location.lastData;
        //Latitude =  Math.Round(last.latitude, 6);
        //Longitude =  Math.Round(last.longitude, 6);
        // 필요 시: var heading = Input.compass.trueHeading;
    }

    void OnDisable()
    {
        if (Input.location.status == LocationServiceStatus.Running)
            Input.location.Stop();
        if (Input.compass.enabled)
            Input.compass.enabled = false;
    }

    // ===== 거리/방위 계산 유틸 =====
    public enum DistUnit { kilometer, meter }

    /// <summary>
    /// 두 좌표 간 거리(단위: m/km). 원본 로직(구면 코사인 법칙) 유지.
    /// </summary>
    public static double Distance(double lat1, double lon1, double lat2, double lon2, DistUnit unit)
    {
        double theta = lon1 - lon2;
        double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) +
                      Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));

        dist = Math.Acos(dist);
        dist = Rad2Deg(dist);
        dist = dist * 60 * 1.1515; // miles

        if (unit == DistUnit.kilometer) dist *= 1.609344;
        else if (unit == DistUnit.meter) dist *= 1609.344;

        return dist;
    }

    /// <summary>
    /// 현재 위치에서 목표 좌표까지 거리(m 기준 기본)
    /// </summary>
    public double DistanceTo(double targetLat, double targetLon, DistUnit unit = DistUnit.meter)
        => Distance(Latitude, Longitude, targetLat, targetLon, unit);

    /// <summary>
    /// 초기 방위각(현재→목표, 0~360)
    /// </summary>
    public static double InitialBearing(double lat1, double lon1, double lat2, double lon2)
        => (Bearing(lat1, lon1, lat2, lon2) + 360.0) % 360.0;

    /// <summary>
    /// 최종 방위각(목표→현재 기준의 역방위 + 180°, 0~360)
    /// </summary>
    public static double FinalBearing(double lat1, double lon1, double lat2, double lon2)
        => (Bearing(lat2, lon2, lat1, lon1) + 180.0) % 360.0;

    static double Bearing(double lat1, double lon1, double lat2, double lon2)
    {
        double phi1 = Deg2Rad(lat1);
        double phi2 = Deg2Rad(lat2);
        double lam1 = Deg2Rad(lon1);
        double lam2 = Deg2Rad(lon2);

        return Math.Atan2(Math.Sin(lam2 - lam1) * Math.Cos(phi2),
                          Math.Cos(phi1) * Math.Sin(phi2) -
                          Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(lam2 - lam1)
               ) * 180.0 / Math.PI;
    }
    
    static double Deg2Rad(double deg) => deg * Math.PI / 180.0;
    static double Rad2Deg(double rad) => rad * 180.0 / Math.PI;
}
