using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class GPS : MonoBehaviour
{
    // ���� ��ǥ(�б��)
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    // �ڵ� ���� ����
    [SerializeField] private bool autoStart = true;

    IEnumerator Start()
    {
#if UNITY_ANDROID
        // �ȵ���̵� ��ġ ���� ��û
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

        // ��Ȯ��(distanceFilter 0.2m, 0.2m ���� ������Ʈ)
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

        // ��ħ��(������) �ʿ� �� Ȱ��ȭ �� �� ����
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
        // �ʿ� ��: var heading = Input.compass.trueHeading;
    }

    void OnDisable()
    {
        if (Input.location.status == LocationServiceStatus.Running)
            Input.location.Stop();
        if (Input.compass.enabled)
            Input.compass.enabled = false;
    }

    // ===== �Ÿ�/���� ��� ��ƿ =====
    public enum DistUnit { kilometer, meter }

    /// <summary>
    /// �� ��ǥ �� �Ÿ�(����: m/km). ���� ����(���� �ڻ��� ��Ģ) ����.
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
    /// ���� ��ġ���� ��ǥ ��ǥ���� �Ÿ�(m ���� �⺻)
    /// </summary>
    public double DistanceTo(double targetLat, double targetLon, DistUnit unit = DistUnit.meter)
        => Distance(Latitude, Longitude, targetLat, targetLon, unit);

    /// <summary>
    /// �ʱ� ������(������ǥ, 0~360)
    /// </summary>
    public static double InitialBearing(double lat1, double lon1, double lat2, double lon2)
        => (Bearing(lat1, lon1, lat2, lon2) + 360.0) % 360.0;

    /// <summary>
    /// ���� ������(��ǥ������ ������ ������ + 180��, 0~360)
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
