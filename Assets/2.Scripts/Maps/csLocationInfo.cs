using Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csLocationInfo : MonoBehaviour
{
    [SerializeField] private Button setStartLocationButton;
    [SerializeField] private Button setEndLocationButton;
    [SerializeField] private TextMeshProUGUI locationNameTMP;
    [SerializeField] private TextMeshProUGUI locationDistanceTMP;


    public void Init(LocationData data)
    {
        this.gameObject.SetActive(true);
        // 각각텍스트나 버튼에 연결
        // 버튼은 출발을 오프셋1, 도착을 오프셋2 data넘기기

        setStartLocationButton.onClick.RemoveAllListeners();
        setStartLocationButton.onClick.AddListener(() =>
        {
                csMapManager.Instance._searchManager.SetSearchUI(data, 1);
                clear();
        });

        setEndLocationButton.onClick.RemoveAllListeners();
        setEndLocationButton.onClick.AddListener(() =>
        {
                csMapManager.Instance._searchManager.SetSearchUI(data, 2);
                clear();
        });

        locationNameTMP.text = data.GetLocalizedName();

        double distance = csMapManager.Instance.GetDistanceMeters(data.geoCoordinate.Latitude, data.geoCoordinate.Longitude, csMapManager.Instance.MyGPS.Latitude, csMapManager.Instance.MyGPS.Longitude);

        string distanceText;

        if (distance < 1000)
        {
            // 1000m 미만 → 미터 단위
            distanceText = distance.ToString("F0") + " m";
        }
        else
        {
            // 1km 이상 → km 단위
            double km = distance / 1000.0;
            distanceText = km.ToString("F1") + " km";
        }

        // UI 적용
        locationDistanceTMP.text = distanceText;

    }

    public void clear()
    {
        this .gameObject.SetActive(false);

        setStartLocationButton.onClick.RemoveAllListeners();
        setEndLocationButton.onClick.RemoveAllListeners();
    }
}
