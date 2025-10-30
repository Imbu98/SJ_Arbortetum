using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csPlaceSlot : MonoBehaviour
{
    public Button placeSlotClickButton;

    public TextMeshProUGUI placeText;

    [HideInInspector]public string placeName = string.Empty; // 검색 장소 이름

    [HideInInspector]public double placeLatitude = 0; // 검색 장소 위도

    [HideInInspector]public double placelongitude = 0; // 검색 장소 경도

    public void SetProperty(string text,double Latitude,double longitude)
    {
        placeName = text;
        placeText.text = text;
        placeLatitude = Latitude;
        placelongitude = longitude;
    }
}
