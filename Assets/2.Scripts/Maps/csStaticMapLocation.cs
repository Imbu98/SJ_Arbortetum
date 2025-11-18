using Cysharp.Threading.Tasks.Triggers;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csStaticMapLocation : MonoBehaviour
{
    [SerializeField] private LocationData locationData;

    [SerializeField] private TextMeshProUGUI locationText;


    void Start()
    {
        if (locationData == null) return;

        this.GetComponent<Button>().onClick.AddListener(() => csMapManager.Instance._searchManager.SetSearchUI(locationData, 0));

        Vector2 p = csMapManager.Instance.LatLonToRelativePosition(
           locationData.geoCoordinate.Latitude,
           locationData.geoCoordinate.Longitude,
           csMapManager.Instance.centerLat,
           csMapManager.Instance.centerLon,
           csMapManager.Instance.zoom
        );

        Vector2 ui = csMapManager.Instance.RelativeToUIPosition(p, csMapManager.Instance.mapRawImage);

        this.GetComponent<RectTransform>().anchoredPosition = ui;

        locationText.text = locationData.GetLocalizedName();
    }
}
