using Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csLocationInfo : MonoBehaviour
{
    [SerializeField] private Button setStartLocationButton;
    [SerializeField] private Button setEndLocationButton;
    [SerializeField] private Button closeLocationInfoButton;
    [SerializeField] private TextMeshProUGUI locationNameTMP;
    [SerializeField] private TextMeshProUGUI locationDistanceTMP;

    [SerializeField] private Image locationImage;



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

        closeLocationInfoButton.onClick.RemoveAllListeners();
        closeLocationInfoButton.onClick.AddListener(()=>csMapManager.Instance._searchManager.ClearSearchUI());

        locationNameTMP.text = data.GetLocalizedName();

        double distance = csMapManager.Instance.GetDistanceMeters(data.geoCoordinate, csMapManager.Instance.GetMyGPS());

        // UI 적용
        locationDistanceTMP.text = csMapManager.Instance.DistanceToText(distance);

        LoadLocationImage(data.locationID);
    }

    private void LoadLocationImage(int locationID)
    {
        // 예: Resources/LocationImages/1.png
        Sprite sprite = Resources.Load<Sprite>($"LocationImages/{locationID}");

        if (sprite != null)
        {
            locationImage.sprite = sprite;
            locationImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"⚠️ 이미지 없음: LocationImages/{locationID}");
            // 필요하면 placeholder 이미지
            locationImage.sprite = Resources.Load<Sprite>("LocationImages/default");
            locationImage.color = Color.gray;
        }
    }

    public void clear()
    {
        this .gameObject.SetActive(false);

        setStartLocationButton.onClick.RemoveAllListeners();
        setEndLocationButton.onClick.RemoveAllListeners();
    }
}
