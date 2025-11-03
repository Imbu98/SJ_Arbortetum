using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csLocationInfo : MonoBehaviour
{
    [SerializeField] private Button setStartLocationButton;
    [SerializeField] private Button setEndLocationButton;
    [SerializeField] private TextMeshProUGUI locationNameTMP;


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
    }

    public void clear()
    {
        this .gameObject.SetActive(false);
    }
}
