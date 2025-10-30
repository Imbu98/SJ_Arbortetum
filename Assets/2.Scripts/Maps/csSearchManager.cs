using Data;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class csSearchManager : MonoBehaviour
{
    [SerializeField] private GameObject searchScreen; // 장소검색 시 띄울 화면

    [SerializeField] private GameObject searchPlaceObject; // 장소 검색을 위한 오브젝트
    [SerializeField] private GameObject pathFindObject; // 길찾기를 위한 오브젝트

    [HideInInspector] public string searchLocation = ""; // 현재 찾으려는 장소 문자열
    [HideInInspector] public string pathFind_StartLocation = ""; // 길찾기 출발지 장소 문자열
    [HideInInspector] public string pathFind_EndLocation = "";   // 길찾기 도착지 장소 문자열

    [Header("InutFields")]
    [SerializeField] private TMP_InputField searchScreen_InputField; // 장소 검색 화면의 InputField;
    [SerializeField] private TMP_InputField pathFind_StartInputField; // 길찾기 출발 InputField
    [SerializeField] private TMP_InputField pathFind_EndInputField; // 길찾기 도착 InputField

    [Header("Buttons")]
    [SerializeField] private Button searchScreenButton; // 장소 검색 화면 여는 버튼
    [SerializeField] private Button closeSearchScreen; // 장소검색 화면 닫기 버튼

    private Dictionary<string, List<LocationData>> cachedCSVData = new Dictionary<string, List<LocationData>>();
    private List<csPlaceSlot> activeSuggestions = new List<csPlaceSlot>();

    [SerializeField] private RectTransform SearchPlaceListHolder; // 검색 결과 부모 트랜스폼
    [SerializeField] private csPlaceSlot PlaceSlotPrefab;   // 검색 결과 리스트 프리펩
    [SerializeField] private GameObject NoSearchListObject; // 검색 결과가 없을 때 표시할 오브젝트

    private void OnEnable()
    {
        searchScreen_InputField.onValueChanged.AddListener(OnInputChanged);
        SearchPlaceListHolder.gameObject.SetActive(false);
        searchScreenButton.onClick.AddListener(OnSearchScreenButtonClicked);
        closeSearchScreen.onClick.AddListener(OnCloseScreenButtonClicked);
    }

    private void OnDisable()
    {
        searchScreen_InputField.onValueChanged.RemoveAllListeners();
        searchScreenButton.onClick.RemoveAllListeners();
        closeSearchScreen.onClick.RemoveAllListeners();
    }

    // 지도 화면에서 길찾기 화면 여는 버튼 클릭
    private void OnSearchScreenButtonClicked()
    {
        searchScreen.gameObject.SetActive(true);
    }

    // 길찾기 화면에서 나가기 버튼 클릭
    private void OnCloseScreenButtonClicked()
    {
        searchScreen.gameObject.SetActive(false);
        searchScreen_InputField.text = string.Empty;
        searchScreenButton.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;

    }
    private void OnInputChanged(string input)
    
    {
        ClearSuggestions();

        string currentLang = csSingleton.Instance.languageCode;
        var results =  csSingleton.Instance.Search(input, currentLang);

        // 검색창이 비어있지 않거나 결과가 나왔을 때 
        if (results.Count>0 )
        {
            SetListIfExist(true);

            foreach (var data in results)
            {
                var suggestion = Instantiate(PlaceSlotPrefab, SearchPlaceListHolder);
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
        searchScreen.gameObject.SetActive(false); // 검색화면 off
        searchScreenButton.GetComponentInChildren<TextMeshProUGUI>().text = locationName; // 지도 화면의 버튼 텍스트 현재 장소 이름으로 변경

        // 지도를 해당 위치로 이동
        csMapManager.Instance.MoveMapToLocation(data.Latitude,data.Longitude);

        Debug.Log($"선택됨: {locationName} / 위도: {data.Latitude}, 경도: {data.Longitude}");
    }

    private void ClearSuggestions()
    {
        activeSuggestions.Clear();

        foreach (Transform child in SearchPlaceListHolder.transform) Destroy(child.gameObject);

        activeSuggestions.Clear();
    }
    private void SetListIfExist(bool IsExist)
    {
        SearchPlaceListHolder.gameObject.SetActive(IsExist);
        NoSearchListObject.gameObject.SetActive(!IsExist);


    }



}
