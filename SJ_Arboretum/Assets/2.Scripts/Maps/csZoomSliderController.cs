using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class csZoomSliderController : MonoBehaviour
{
    [SerializeField] private RawImage mapRawImage;
    [SerializeField] private Button zoomPlusButton;
    [SerializeField] private Button zoomMinusButton;

    [SerializeField] private int minZoom = 15;
    [SerializeField] private int maxZoom = 20;
   

    private Vector3 baseScale;
    private void Start()
    {
        zoomPlusButton.onClick.AddListener(() => OnClickZoomButton(true));
        zoomMinusButton.onClick.AddListener(() => OnClickZoomButton(false));
    }

    private void OnDisable()
    {
        zoomPlusButton.onClick.RemoveAllListeners();
        zoomMinusButton.onClick.RemoveAllListeners();
    }

    private void OnClickZoomButton(bool isPlus)
    {
        if(isPlus)
        {
            if(csMapManager.Instance.currentZoom < maxZoom)
            {
                csMapManager.Instance.currentZoom += 1;
            }
        }
        else
        {
            if(csMapManager.Instance.currentZoom > minZoom)
            {
                csMapManager.Instance.currentZoom -= 1;
            }
        }

        double latitude = csMapManager.Instance.save_latitude;
        double longitude = csMapManager.Instance.save_longitude;
        csMapManager.Instance.LoadMap(csMapManager.Instance.currentZoom,latitude,longitude);
        
    }
}
