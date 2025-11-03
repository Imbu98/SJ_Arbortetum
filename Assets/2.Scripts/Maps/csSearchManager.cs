using Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    [SerializeField] private Button StartPathFindButton; // 길찾기 시작 버튼
    [SerializeField] private Button pathFind_StartButton; // 출발 장소 검색 화면 여는 버튼 
    [SerializeField] private Button pathFind_EndButton; // 도착 장소 검색 화면 여는 버튼 
    [SerializeField] private Button closePathFindButton; // 길찾기상태에서 지도 초기화면으로 돌아가는 버튼


    [SerializeField] private csLocationInfo locationInfo; // 장소 검색시 아래에 띄울 장소 정보 UI






    private void OnEnable()
    {
        pathFindButton.onClick.AddListener(() => SetPathFindUI(true));
        resetSearchButton.onClick.AddListener(ClearSearchUI);
        searchScreenButton.onClick.AddListener(() => OnSearchScreenButtonClicked(0, searchScreenButton));


        StartPathFindButton.onClick.AddListener(() => csMapManager.Instance.EsearchStatus = SearchStatus.SearchPath);
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

        StartPathFindButton.onClick.RemoveAllListeners();
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
    public async Task SetSearchUI(LocationData data, int offset)
    {
        csMapManager.Instance.EsearchStatus = SearchStatus.None;

        // 지도를 해당 위치로 이동
        csMapManager.Instance.MoveMapToLocation(data.geoCoordinate.Latitude, data.geoCoordinate.Longitude, offset);

        if (offset == 0)
        {
            // 장소 정보 UI띄우기
            locationInfo.Init(data);
            // 검색창 UI 버튼 변경
            SetSearchScreenButtonUI(true);
            // 현재 검색 장소 정보 저장
            searchLocationData = data;
            // 텍스트 표시
            searchScreenButton.GetComponentInChildren<TextMeshProUGUI>().text = data.GetLocalizedName();
        }
        else
        {
            SetPathFindUI(true);
            if (offset == 1)
            {
                pathFind_StartLocationData = data;

                pathFind_StartButton.GetComponentInChildren<TextMeshProUGUI>().text = data.GetLocalizedName();
            }
            else if (offset == 2)
            {
                pathFind_EndLocationData = data;

                pathFind_EndButton.GetComponentInChildren<TextMeshProUGUI>().text = data.GetLocalizedName();

                // 도착지점에 정보 넣었는데 시작지점에 정보가 없으면 자동으로 내 위치를 정보로 저장
                if (!IsValidLocation(pathFind_StartLocationData))
                {
                    csMapManager.Instance.EsearchStatus = SearchStatus.SearchPath;

                    pathFind_StartLocationData = new LocationData
                    {
                        geoCoordinate = new GeoCoordinate(csMapManager.Instance.MyGPS.Latitude, csMapManager.Instance.MyGPS.Longitude),
                        koreanName = "내 위치",
                        englishName = "My Location",
                        locationID = -1

                    };
                    pathFind_StartButton.GetComponentInChildren<TextMeshProUGUI>().text = pathFind_StartLocationData.GetLocalizedName();
                    
                }
            }
        }
        // 두 좌표다 유효하면 바로 길찾기
        if (IsValidLocation(pathFind_StartLocationData) && IsValidLocation(pathFind_EndLocationData))
        {

            // 서버에서 AI한테 경로 좌표 받아와야함
            // 일단 임시로 테스트
           await csNetworkManager.Instance.GetDestinationCoordsAsync(pathFind_StartLocationData.geoCoordinate, pathFind_EndLocationData.geoCoordinate);

            List<GeoCoordinate> coords = new List<GeoCoordinate>();
            GeoCoordinate endCoord = new GeoCoordinate(
        pathFind_EndLocationData.geoCoordinate.Latitude,
        pathFind_EndLocationData.geoCoordinate.Longitude
    );
            coords.Add(endCoord);

            csMapManager.Instance.SearchPath(pathFind_StartLocationData.geoCoordinate, coords);
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
        pathFind_StartLocationData = null;
        pathFind_EndLocationData = null;
        // 하드코딩 되어있는데 나중에 변경해야함
        pathFind_StartButton.GetComponentInChildren<TextMeshProUGUI>().text = "출발지";
        pathFind_EndButton.GetComponentInChildren<TextMeshProUGUI>().text = "도착지";
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
        if (loc == null) return false;
        //위도/경도가 0이 아니면 유효하다고 판단
        return loc != null && loc.geoCoordinate.Latitude != 0 && loc.geoCoordinate.Longitude != 0;
    }

    private void OnReverseDestinationButton()
    {
        csMapManager.Instance.DestroyPathFindPrefab();
        // 데이터 자체를 스왑 (유효하지 않아도 그대로 교체)
        LocationData temp = pathFind_StartLocationData;
        pathFind_StartLocationData = pathFind_EndLocationData;
        pathFind_EndLocationData = temp;

        // 도착지점이 내 위치면 길찾기 취소, 아니면 길찾기
        csMapManager.Instance.EsearchStatus = pathFind_EndLocationData.locationID == -1 ? SearchStatus.None : SearchStatus.SearchPath;

            // 현재 언어 코드 (예: "ko", "en")
            string languageCode = csSingleton.Instance.languageCode; // 또는 현재 사용하는 언어 변수

        // 버튼 텍스트 교체 (없으면 기본 문구로)
        TextMeshProUGUI startText = pathFind_StartButton.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI endText = pathFind_EndButton.GetComponentInChildren<TextMeshProUGUI>();

        // 언어별 기본 텍스트 설정
        string defaultStart = (languageCode == "ko") ? "출발지" : "Start";
        string defaultEnd = (languageCode == "ko") ? "도착지" : "End";

        string newStartText = pathFind_StartLocationData != null && IsValidLocation(pathFind_StartLocationData)
            ? pathFind_StartLocationData.GetLocalizedName()
            : defaultStart;

        string newEndText = pathFind_EndLocationData != null && IsValidLocation(pathFind_EndLocationData)
            ? pathFind_EndLocationData.GetLocalizedName()
            : defaultEnd;

        startText.text = newStartText;
        endText.text = newEndText;

        // 지도 이동 (새 출발지가 유효할 때만)
        if (pathFind_StartLocationData != null && IsValidLocation(pathFind_StartLocationData))
        {
            csMapManager.Instance.MoveMapToLocation(
                pathFind_StartLocationData.geoCoordinate.Latitude,
                pathFind_StartLocationData.geoCoordinate.Longitude,
                1
            );
        }

        // 두 좌표가 모두 유효하면 바로 길찾기
        if (IsValidLocation(pathFind_StartLocationData) && IsValidLocation(pathFind_EndLocationData))
        {
            List<GeoCoordinate> coords = new List<GeoCoordinate>
        {
            new GeoCoordinate(
                pathFind_EndLocationData.geoCoordinate.Latitude,
                pathFind_EndLocationData.geoCoordinate.Longitude
            )
        };

            csMapManager.Instance.SearchPath(pathFind_StartLocationData.geoCoordinate, coords);
        }

        Debug.Log("✅ 출발지와 도착지를 교체했습니다. (유효하지 않아도 처리됨)");
    }
}
