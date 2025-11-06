using Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class csSearchManager : MonoBehaviour
{
    [SerializeField] private GameObject searchPlaceObject; // 장소 검색을 위한 오브젝트
    [SerializeField] private GameObject pathFindObject; // 길찾기를 위한 오브젝트

    [HideInInspector] public LocationData searchLocationData = new LocationData();
    [HideInInspector] public LocationData pathFind_StartLocationData = new LocationData();
    [HideInInspector] public LocationData pathFind_EndLocationData = new LocationData();

    [Header("Buttons")]
    [SerializeField] private Button pathFindButton; // 길찾기 UI 전환 버튼
    [SerializeField] private Button resetSearchButton; // 검색한 장소가 있는 상태에서 지도 초기화면으로 돌아가는 버튼
    [SerializeField] private Button searchScreenButton; // 장소 검색 화면 여는 버튼
    [SerializeField] private Button ReverseDestinationButton; // 출발지, 도착지 변경 버튼

    [SerializeField] private Button pathFind_StartButton; // 출발 장소 검색 화면 여는 버튼 
    [SerializeField] private Button pathFind_EndButton; // 도착 장소 검색 화면 여는 버튼 
    [SerializeField] private Button closePathFindButton; // 길찾기상태에서 지도 초기화면으로 돌아가는 버튼


    [SerializeField] private csLocationInfo locationInfo; // 장소 검색시 아래에 띄울 장소 정보 UI






    private void OnEnable()
    {
        pathFindButton.onClick.AddListener(() => SetPathFindUI(true));
        resetSearchButton.onClick.AddListener(ClearSearchUI);
        searchScreenButton.onClick.AddListener(() => OnSearchScreenButtonClicked(0, searchScreenButton));


        // 길찾기 출발지 변경
        pathFind_StartButton.onClick.AddListener(() => OnSearchScreenButtonClicked(1, pathFind_StartButton));
        // 길찾기 목적지 변경
        pathFind_EndButton.onClick.AddListener(() => OnSearchScreenButtonClicked(2, pathFind_EndButton));
        // 길찾기 닫기 버튼
        closePathFindButton.onClick.AddListener(() => SetPathFindUI(false));

        // 출발지, 목적지 바꾸기
        ReverseDestinationButton.onClick.AddListener(OnReverseDestinationButton);


    }

    private void OnDisable()
    {
        pathFindButton.onClick.RemoveAllListeners();
        resetSearchButton.onClick.RemoveAllListeners();
        searchScreenButton.onClick.RemoveAllListeners();

        pathFind_StartButton.onClick.RemoveAllListeners();
        pathFind_EndButton.onClick.RemoveAllListeners();
        closePathFindButton.onClick.RemoveAllListeners();

        ReverseDestinationButton.onClick.RemoveAllListeners();
    }

    // 지도 화면에서 장소 검색 화면 열기
    private void OnSearchScreenButtonClicked(int offset, Button currentButton)
    {
        csMapManager.Instance._searchScreen.OpenSearchScreen(offset, currentButton);
    }

    // 검색화면에서 장소리스트중 하나를 클릭했을 때
    public void SetSearchUI(LocationData data, int offset)
    {
        csMapManager.Instance.E_searchStatus = SearchStatus.None;

        // 지도를 해당 위치로 이동
        csMapManager.Instance.MoveMapToLocation(data.geoCoordinate.Latitude, data.geoCoordinate.Longitude, offset);

        switch (offset)
        {
            case 0:
                locationInfo.Init(data);
                SetSearchScreenButtonUI(true);
                searchLocationData = data;
                searchScreenButton.GetComponentInChildren<TextMeshProUGUI>().text = data.GetLocalizedName();
                break;

            case 1:
                pathFind_StartLocationData = data;
                UpdatePathButtonText(pathFind_StartButton, data);
                SetPathFindUI(true);
                break;

            case 2:
                pathFind_EndLocationData = data;
                SetPathFindUI(true);
                // 출발지가 없으면 내 위치 자동 설정
                if (!IsValidLocation(pathFind_StartLocationData))
                {
                    pathFind_StartLocationData = CreateMyLocation();
                    UpdatePathButtonText(pathFind_StartButton, pathFind_StartLocationData);
                }
                UpdatePathButtonText(pathFind_EndButton, data);

                TryStartPathFinding();
                break;
        }
        
    }

    // 검색장소 초기화
    public void ClearSearchUI()
    {
        csMapManager.Instance.ClearSearchLocation();

        // 검색 장소  정보 초기화
        locationInfo.clear();

        // 
        searchScreenButton.GetComponentInChildren<TextMeshProUGUI>().text = "검색할 장소를 입력하세요";
    }

    public void ClearPathFindUI()
    {
        // 길찾기 데이터 초기화
        pathFind_StartLocationData = new LocationData();
        pathFind_EndLocationData = new LocationData();
        // 하드코딩 되어있는데 나중에 변경해야함
        UpdatePathButtonText(pathFind_StartButton, pathFind_StartLocationData);
        UpdatePathButtonText(pathFind_EndButton, pathFind_EndLocationData);

        searchScreenButton.GetComponentInChildren<TextMeshProUGUI>().text = "검색할 장소를 입력하세요";
    }

    // 길찾기인지, 장소검색인지에 따라 다른 object 활성화
    public void SetPathFindUI(bool IsPathFind)
    {
        // UI전환
        searchPlaceObject.SetActive(!IsPathFind);
        pathFindObject.SetActive(IsPathFind);

        // 길찾기에서 기본상태로 전환
        if (IsPathFind == false)
        {
            // 기존 길찾기 관련 정보 초기화
            csMapManager.Instance.ClearPathFindUI();
            csMapManager.Instance.DestroyPathFindPrefab();
        }


    }

    // 현재 검색한 장소가 있는지에 따라 다른 UI활성화
    public void SetSearchScreenButtonUI(bool IsOnSearch)
    {
        resetSearchButton.gameObject.SetActive(IsOnSearch);
        pathFindButton.gameObject.SetActive(!IsOnSearch);
    }

    //시작 정보와 도착 정보 둘 다 있어야 길찾기 시작
    private bool IsValidLocation(LocationData loc)
    {
        if (loc.locationID == 0 ) return false;
        //위도/경도가 0이 아니면 유효하다고 판단
        return loc.locationID != 0; 
    }

    // locationID가 -1이면 현재 내 위치로 갱신
    private void RefreshMyLocationIfNeeded(LocationData loc)
    {
        if (loc != null && loc.locationID == -1)
        {
            loc.geoCoordinate.Latitude = csMapManager.Instance.MyGPS.Latitude;
            loc.geoCoordinate.Longitude = csMapManager.Instance.MyGPS.Longitude;
        }
    }

    // 비어있으면 내 위치를 기준으로 LocationData를 새로 만듦
    private LocationData CreateMyLocation()
    {
        return new LocationData
        {
            geoCoordinate = new GeoCoordinate(csMapManager.Instance.MyGPS.Latitude, csMapManager.Instance.MyGPS.Longitude),
            koreanName = "내 위치",
            englishName = "My Location",
            locationID = -1
        };
    }
    // 버튼 텍스트 업데이트
    private void UpdatePathButtonText(Button button, LocationData data)
    {
        string text;

        if (IsValidLocation(data))
        {
            text = data.GetLocalizedName();
        }
        else
        {
            // 버튼 종류에 따라 Localization Key 선택
            string key = (button == pathFind_StartButton)
                ? "Key_StartLocation"
                : "Key_EndLocation";

            text = csLocalizationManager.Instance.LocalizationString(key);
        }

        // 버튼 텍스트 변경
        button.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    // 서버에 길찾기 요청
    private async void TryStartPathFinding()
    {
        if (!IsValidLocation(pathFind_StartLocationData) || !IsValidLocation(pathFind_EndLocationData))
            return;

        RefreshMyLocationIfNeeded(pathFind_StartLocationData);
        RefreshMyLocationIfNeeded(pathFind_EndLocationData);

        var searchPath = await csNetworkManager.Instance.GetDestinationCoordsAsync(
            pathFind_StartLocationData.geoCoordinate, pathFind_EndLocationData.geoCoordinate);

        // 시작지점이 내 위치(id==-1)이면 길찾기, 아니면 길찾기 중지
        csMapManager.Instance.E_searchStatus = pathFind_StartLocationData.locationID == -1 ? SearchStatus.SearchPath : SearchStatus.None;

        csMapManager.Instance.SearchPath(pathFind_StartLocationData, searchPath.pathCoordinates);
    }

    // 출발지와 목적지 전환
    private void OnReverseDestinationButton()
    {
        csMapManager.Instance.DestroyPathFindPrefab();
        // 데이터 자체를 스왑 (유효하지 않아도 그대로 교체)
        LocationData temp = pathFind_StartLocationData;
        pathFind_StartLocationData = pathFind_EndLocationData;
        pathFind_EndLocationData = temp;

        // 스왑했는데 출발지가 없으면 내 위치로 지정
        if (!IsValidLocation(pathFind_StartLocationData))
        {
            csMapManager.Instance.E_searchStatus = SearchStatus.SearchPath;

            pathFind_StartLocationData = CreateMyLocation();
        }


        // 현재 언어 코드 (예: "ko", "en")
        string languageCode = csSingleton.Instance.languageCode; // 또는 현재 사용하는 언어 변수

        // 버튼 텍스트 교체 (없으면 기본 문구로)
        TextMeshProUGUI startText = pathFind_StartButton.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI endText = pathFind_EndButton.GetComponentInChildren<TextMeshProUGUI>();

        UpdatePathButtonText(pathFind_StartButton, pathFind_StartLocationData);
        UpdatePathButtonText(pathFind_EndButton, pathFind_EndLocationData);

        // 지도 이동 (새 출발지가 유효할 때만)
        if (IsValidLocation(pathFind_StartLocationData))
        {
            csMapManager.Instance.MoveMapToLocation(
                pathFind_StartLocationData.geoCoordinate.Latitude,
                pathFind_StartLocationData.geoCoordinate.Longitude,
                1
            );
        }

        TryStartPathFinding();

        Debug.Log("✅ 출발지와 도착지를 교체했습니다. (유효하지 않아도 처리됨)");
    }
}
