using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csSearchScreen : MonoBehaviour
{
    [Header("InutFields")]
    [SerializeField] private TMP_InputField searchScreen_InputField; // 장소 검색 화면의 InputField;

    [Header("SearchList")]
    [SerializeField] private RectTransform searchPlaceListHolder; // 검색 결과 부모 트랜스폼
    [SerializeField] private csPlaceSlot placeSlotPrefab;   // 검색 결과 리스트 프리펩
    [SerializeField] private GameObject NoSearchListObject; // 검색 결과가 없을 때 표시할 오브젝트

    [Header("Buttons")]
    [SerializeField] private Button closeSearchScreen; // 장소검색 화면 닫기 버튼

    private Button currentButton; // 어떤 버튼으로 검색화면을 열었는지 저장
    private string currentButtonString = string.Empty;
    private int currentOffset = 0; // 현재 어떤 검색인지 확인하는 변수

    private Dictionary<string, List<LocationData>> cachedCSVData = new Dictionary<string, List<LocationData>>();
    private List<csPlaceSlot> activeSuggestions = new List<csPlaceSlot>();

    private void OnEnable()
    {
        searchScreen_InputField.onValueChanged.AddListener(OnInputChanged);
        searchPlaceListHolder.gameObject.SetActive(false);
        closeSearchScreen.onClick.AddListener(OnCloseScreenButtonClicked);
    }

    private void OnDisable()
    {
        searchScreen_InputField.onValueChanged.RemoveAllListeners();
        closeSearchScreen.onClick.RemoveAllListeners();
    }

    // 검색화면 열기
    public void OpenSearchScreen(int offset, Button currentButton)
    {
        this.gameObject.SetActive (true);
        this.currentOffset = offset;
        this.currentButton = currentButton;
        this.currentButtonString = currentButton.GetComponentInChildren<TextMeshProUGUI>().text;
    }

    // 검색화면 닫기
    private void OnCloseScreenButtonClicked()
    {
        this.gameObject.SetActive(false);
        searchScreen_InputField.text = string.Empty; // 검색화면 InputField 초기화
        currentButton.GetComponentInChildren<TextMeshProUGUI>().text = currentButtonString; // 눌렀던 버튼의 텍스트 기존 텍스트로 초기화
        csMapManager.Instance.ClearSearchLocation();

    }


    private void OnInputChanged(string input)
    {
        ClearSuggestions();

        string currentLang = csSingleton.Instance.languageCode;
        var results = csSingleton.Instance.Search(input, currentLang);

        // 검색창이 비어있지 않거나 결과가 나왔을 때 
        if (results.Count > 0)
        {
            SetListIfExist(true);

            foreach (var data in results)
            {
                var suggestion = Instantiate(placeSlotPrefab, searchPlaceListHolder);
                // 현재 언어에 맞게 표시 이름 선택
                suggestion.GetComponentInChildren<TextMeshProUGUI>().text = data.GetLocalizedName();
                var closureData = data;

                suggestion.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnSuggestionClicked(closureData);
                });
                activeSuggestions.Add(suggestion);
            }
        }
        else
        {
            SetListIfExist(false);
        }
    }

    // 검색 결과 중 하나가 클릭되었을 때
    private void OnSuggestionClicked(LocationData data)
    {
        string locationName = data.GetLocalizedName();
        searchScreen_InputField.text = locationName; // 검색화면 inputfield text변경 ( 다시 켯을 때 그대로 남아있도록)
        this.gameObject.SetActive(false); // 검색화면 off
        currentButton.GetComponentInChildren<TextMeshProUGUI>().text = locationName; // 지도 화면의 버튼 텍스트 현재 장소 이름으로 변경

        csMapManager.Instance._searchManager.SetSearchUI(data,currentOffset);


        Debug.Log($"선택됨: {locationName} / 위도: {data.geoCoordinate.Latitude}, 경도: {data.geoCoordinate.Longitude}");
    }

    private void ClearSuggestions()
    {
        activeSuggestions.Clear();

        foreach (Transform child in searchPlaceListHolder.transform) Destroy(child.gameObject);

        activeSuggestions.Clear();
    }
    private void SetListIfExist(bool IsExist)
    {
        searchPlaceListHolder.gameObject.SetActive(IsExist);
        NoSearchListObject.gameObject.SetActive(!IsExist);
    }

}
