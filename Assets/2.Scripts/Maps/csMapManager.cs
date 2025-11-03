using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Networking;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class csMapManager : MonoBehaviour
{
    public static csMapManager Instance { get { return _Instance; } }
    private static csMapManager _Instance;

    private bool IsMapOpened = false;   // 지도가 켜져있는지 확인하는 변수

    public SearchStatus EsearchStatus=SearchStatus.None; // 현재 길을 찾는 중인지 나타내는 변수

    public csSearchManager _searchManager;
    public csSearchScreen _searchScreen;

    public SearchPathCoordinate currentGeoCoordinate; // 현재 목적지까지 가는 좌표 리스트

    private int CurrentTargetCoordnateIndex; // 현재 목적지 좌표 인덱스 ( 0에는 시작 위치 , 목적지는 1부터 시작)

    
    // 맵 이미지
    public RawImage mapRawImage;

    // 길 찾기
    [Header("PathFind")]
    [SerializeField] private Image linePrefab;// 각 좌표 사이를 선으로 잇기 위한프리펩
    [SerializeField] private Image startMarkerPrefab; // 시작 지점 마커 프리펩
    [SerializeField] private Image endMarkerPrefab; // 도착 지점 마커 프리펩
    [SerializeField] private Image SearchLocationMarkerPrefab; //검색 결과 지점 마커 프리펩 
    private List<Image> lineList=new List<Image>(); // 저장 후 삭제할 선 리스트
    private float lineSize = 20f; // 선 두께
    [SerializeField] private List<Color> lineColors; // 선 색상
    private Image startMarker; // 시작지점 마커 프리팹 저장
    private Image endMarker; // 도착지점 마커 프리팹 저장
    private Image SearchLocationMarker; // 검색 결과 지점 마커 프리팹 저장

    private Coroutine drawPathCoroutine; // 길찾기 코루틴 참조


    




    // 에디터 테스트용 GPS 좌표 리스트
    public List<GeoCoordinate> gpsList = new List<GeoCoordinate>();

    [Header("Naver API")]
    // 네이버 API 지도 설정 관련 변수
    public string geocodeApiUrl = ""; // 네이버 지도 API의 지오코드 요청 URL    
    public string mapStaticApiUrl = ""; // 네이버 지도 API의 정적 지도 요청 URL
    public string clientID = "";// 네이버 클라우드 플랫폼에서 발급받은 클라이언트 아이디
    public string clientSecret = ""; // 네이버 클라우드 플랫폼에서 발급받은 클라이언트 시크릿
    public double centerLat; // 지도 중심 위도
    public double centerLon; // 지도 중심 경도

    public int zoom = 16; // 지도 줌 레벨
    public int mapWidth; // 지도 이미지 가로 크기
    public int mapHeight; // 지도 이미지 세로 크기

    // 사용자의 현재 위도와 경도 가져오기 위한 변수
    public GPS MyGPS;
    private double latitude;
    private double longitude;
    [HideInInspector]public double save_latitude;
    [HideInInspector] public double save_longitude;
    [SerializeField] private RectTransform markerRect; // 내 위치 마커
    [SerializeField] private RectTransform arrowRect; // 방향 화살표 

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForGPSReady());
        

    }
    private void FixedUpdate()
    {
        if(IsMapOpened)
        {
            UpdateMarkerOnly();
        }
    }
    private void OnEnable()
    {
       
        // 내 위치 마킹
    }

    private void OnDisable()
    {
    }


    private void Update()
    {
        // 길찾기 중일 때
        if(EsearchStatus == SearchStatus.SearchPath&&IsMapOpened)
        {
            LineConnenctToMarker();
            CheckOnArrive();
        }
        //print("location" + latitude + " " + longitude);
    }
    private IEnumerator WaitForGPSReady()
    {
        // GPS가 아직 준비 안된 경우 대기
        while (MyGPS == null || MyGPS.Latitude == 0 || MyGPS.Longitude == 0)
        {
            Debug.Log("GPS 데이터를 기다리는 중...");
            yield return new WaitForSeconds(0.5f);
        }

        LoadMap(zoom);
        save_latitude = MyGPS.Latitude;
        save_longitude = MyGPS.Longitude;
    }

    // 구글 API로 지도 불러오는 함수
    public async void LoadMap(int zoomAmount)
    {
        //mapWidth = Screen.width;
        //mapHeight = Screen.height;

        // 네이버 지도 API 요청 URL 생성
        string apiUrl = $"{mapStaticApiUrl}?w={mapWidth}&h={mapHeight}&center={centerLon},{centerLat}&level={zoom}&Scale=2";
        // 지도 타일 요청
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(apiUrl);
        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientID);
        request.SetRequestHeader("X-NCP-APIGW-API-KEY",clientSecret);

        Debug.Log(apiUrl);

        await request.SendWebRequest(); //req값 반환

        if (request.result != UnityWebRequest.Result.Success)
        {
            
            Debug.LogError("Map Load Error: " + request.error + " | " + request.downloadHandler.text);
            return;
        }

        mapRawImage.texture = DownloadHandlerTexture.GetContent(request); // 맵 >> 이미지에 적용
    }

    public void SetMapOpened(bool isOpened)
    {
        IsMapOpened = isOpened;
        Debug.Log("IsMapOpened 상태 변경: " + IsMapOpened);
    }

    // 에디터 테스트용 버튼 (현재 위치부터 경로 설정)
    public void OnPathButtonPressed()
    {
        // 현재 GPS 위치를 시작점으로 설정
        GeoCoordinate startCoord = new GeoCoordinate(MyGPS.Latitude, MyGPS.Longitude);

        // 현재 경로 리스트(GPSList)가 존재하는지 체크
        if (gpsList == null || gpsList.Count == 0)
        {
            Debug.LogWarning("GPSList가 비어 있습니다. 경로 데이터를 먼저 설정하세요.");
            return;
        }
        EsearchStatus = SearchStatus.SearchPath;
            // SearchPath 호출
            //csMapManager.Instance.SearchPath(startCoord, gpsList);

    }

    public void SearchPath(LocationData startLocationData, List<GeoCoordinate> coords)
    {
        
        if(EsearchStatus == SearchStatus.SearchPath)
        {
            // 내 위치를 기반으로 길찾기 상태중이면 시작마커 삭제
            if (startMarker) Destroy(startMarker.gameObject);
        }

        foreach (var lineImage in lineList)
        {
            if (lineImage != null)
                Destroy(lineImage.gameObject);
        }
        lineList.Clear();


        if (drawPathCoroutine != null)
            StopCoroutine(drawPathCoroutine);
        
        

        currentGeoCoordinate = new SearchPathCoordinate();

        // pthCoords에 시작 좌표와 AI에서 받아온 좌표들 추가
        List<GeoCoordinate> pathCoords = new List<GeoCoordinate>();
        pathCoords.Add(startLocationData.geoCoordinate);
        pathCoords.AddRange(coords);

        currentGeoCoordinate.pathCoordinates = pathCoords;

        drawPathCoroutine = StartCoroutine(DrawPathAnimated(pathCoords, centerLat, centerLon));
    }

    // AI에서 받아온 좌표마다 이어주는 함수
    IEnumerator DrawPathAnimated(List<GeoCoordinate> coords, double centerLat, double centerLon)
    {
        

        if (coords == null || coords.Count < 2)
            yield break;

        // 길찾기를 요청하면 기존의 선 제거
        foreach (var lineImage in lineList)
        {
            if (lineImage != null)
                Destroy(lineImage.gameObject);
        }
        lineList.Clear();

        for (int i = 0; i < coords.Count - 1; i++)
        {
            // 위도, 경도를 네이버 지도 기준 상대 좌표로 변환
            Vector2 p1 = LatLonToRelativePosition(coords[i].Latitude, coords[i].Longitude, centerLat, centerLon, zoom);
            Vector2 p2 = LatLonToRelativePosition(coords[i + 1].Latitude, coords[i + 1].Longitude, centerLat, centerLon, zoom);

            // UI 상의 실제 위치로 변환
            Vector2 startUI = RelativeToUIPosition(p1, mapRawImage);
            Vector2 endUI = RelativeToUIPosition(p2, mapRawImage);

            Vector2 dir = endUI - startUI;
            float distance = dir.magnitude;

            Image line = Instantiate(linePrefab, mapRawImage.transform);
            lineList.Add(line);

            line.color = lineColors[1];
            RectTransform rect = line.rectTransform;
            rect.anchoredPosition = startUI;
            rect.sizeDelta = new Vector2(distance, lineSize);
            rect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            // 마지막 좌표일 때 도착 마커 생성
            if (i == coords.Count - 2)
            {
                if (endMarker != null && endMarker.gameObject != null)
                {
                    Destroy(endMarker.gameObject);
                    endMarker = null;
                }
                endMarker = Instantiate(endMarkerPrefab, mapRawImage.transform);
                endMarker.rectTransform.anchoredPosition = endUI;
            }

            //// 선이 그려지는 애니메이션
            //line.fillAmount = 0;
            //float t = 0;
            //while (t < 1)
            //{
            //    t += Time.deltaTime;
            //    line.fillAmount = t;
            //    yield return null;
            //}
            
        }
    }


    // 중심 좌표 기준 상대 픽셀 좌표 계산
    Vector2 LatLonToRelativePosition(double lat, double lon, double centerLat, double centerLon, int zoom)
    {
        Vector2 centerPixel = LatLonToPixel(centerLat, centerLon, zoom);
        Vector2 pointPixel = LatLonToPixel(lat, lon, zoom);
        return pointPixel - centerPixel;
    }

    // UI(RawImage) 좌표로 변환
    Vector2 RelativeToUIPosition(Vector2 relative, RawImage mapImage)
    {
        RectTransform rect = mapImage.rectTransform;
        float scaleX = rect.rect.width / (float)mapImage.texture.width;
        float scaleY = rect.rect.height / (float)mapImage.texture.height;

        return new Vector2(relative.x * scaleX, -relative.y * scaleY);
    }

    // 위도/경도 → 픽셀 좌표 (Mercator Projection)
    Vector2 LatLonToPixel(double lat, double lon, int zoom)
    {
        double siny = Math.Sin(lat * Math.PI / 180.0);
        siny = Math.Min(Math.Max(siny, -0.9999), 0.9999);

        double tileSize = 512;
        double scale = Math.Pow(2, zoom);

        double x = tileSize * (0.5 + lon / 360.0);
        double y = tileSize * (0.5 - Math.Log((1 + siny) / (1 - siny)) / (4 * Math.PI));

        return new Vector2((float)(x * scale), (float)(y * scale));
    }
    private void UpdateMarkerOnly()
    {
        // 맵이 아직 로드되지 않은 경우 예외처리
        if (mapRawImage.texture == null)
            return;

        double lat;
        double lon;


#if UNITY_IOS || UNITY_ANDROID
        //var location = Input.location.lastData;
        //double lat = location.latitude;
        //double lon = location.longitude;
        lat = MyGPS.Latitude;
        lon = MyGPS.Longitude;
#endif
#if UNITY_EDITOR
        lat = MyGPS.Latitude;
        lon = MyGPS.Longitude;
#endif
        // 위도, 경도를 네이버 지도 기준 상대 좌표로 변환
        Vector2 p1 = LatLonToRelativePosition(lat, lon, centerLat, centerLon, zoom);

        // UI 상의 실제 위치로 변환
        Vector2 startUI = RelativeToUIPosition(p1, mapRawImage);

        markerRect.anchoredPosition = startUI;

        UpdateArrowRotation();
    }

    /// <summary>
    /// 맵이 화면을 벗어나지 않도록 위치 제한
    /// </summary>
    public void ClampMapPosition()
    {
       RectTransform mapRect = mapRawImage.rectTransform;
       Vector2 mapSize = mapRect.sizeDelta;
       Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        Vector2 pos = mapRect.anchoredPosition;

        float halfMapWidth = mapSize.x / 2f * mapRect.localScale.x;
        float halfMapHeight = mapSize.y / 2f * mapRect.localScale.y;
        float halfScreenWidth = screenSize.x / 2f;
        float halfScreenHeight = screenSize.y / 2f;

        // 맵이 화면보다 클 경우에만 경계 제한 적용
        if (mapSize.x > screenSize.x)
        {
            float maxX = (halfMapWidth - halfScreenWidth);
            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        }
        else
        {
            pos.x = 0; // 화면보다 작으면 가운데 고정
        }

        if (mapSize.y > screenSize.y)
        {
            float maxY = (halfMapHeight - halfScreenHeight);
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
        }
        else
        {
            pos.y = 0;
        }

        mapRect.anchoredPosition = pos;
    }

    float simulatedHeading = 0f;

    private void UpdateArrowRotation()
    {
#if UNITY_EDITOR
        simulatedHeading += Time.deltaTime * 30f; // 초당 30도 회전
        if (simulatedHeading > 360) simulatedHeading -= 360;
        arrowRect.localRotation = Quaternion.Euler(0, 0, -simulatedHeading);
#endif


#if UNITY_ANDROID || UNITY_IOS
        float heading = Input.compass.trueHeading;
        // 기본: Z축 기준 회전
        arrowRect.localRotation = Quaternion.Euler(0, 0, -heading);

        Debug.Log("Heading: " + Input.compass.trueHeading);
#endif
    }

    private void LineConnenctToMarker()
    {
        // 나중에 isOnPathSearching 체크해서 필요할 때만 실행
        if (currentGeoCoordinate == null)
            return;

        SearchPathCoordinate pathCoordinate = currentGeoCoordinate;
        GeoCoordinate geoCoordinates = pathCoordinate.pathCoordinates[CurrentTargetCoordnateIndex+1];

        // 위도, 경도를 네이버 지도 기준 상대 좌표로 변환
        Vector2 p1 = LatLonToRelativePosition(MyGPS.Latitude,MyGPS.Longitude, centerLat, centerLon, zoom);
        Vector2 p2 = LatLonToRelativePosition(geoCoordinates.Latitude , geoCoordinates.Longitude, centerLat, centerLon, zoom);

        // UI 상의 실제 위치로 변환
        Vector2 startUI = RelativeToUIPosition(p1, mapRawImage);
        Vector2 endUI = RelativeToUIPosition(p2, mapRawImage);

        Vector2 dir = endUI - startUI;
        float distance = dir.magnitude;

        if(lineList.Count > 0)
        {
            Image line = lineList[CurrentTargetCoordnateIndex];
            if(line != null)
            {
                RectTransform rect = line.rectTransform;
                rect.anchoredPosition = startUI;
                rect.sizeDelta = new Vector2(distance, lineSize);
                rect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }
        }
    }


    private void CheckOnArrive()
    {
        // 목적지 도착 체크 로직
        if(currentGeoCoordinate==null)
            return;

        int missionIndex = csMissionManager.Instance.currentMissionIndex;
        SearchPathCoordinate searchPathCoordinate = currentGeoCoordinate;

        double targetLat = searchPathCoordinate.pathCoordinates[CurrentTargetCoordnateIndex+1].Latitude;
        double targetLon = searchPathCoordinate.pathCoordinates[CurrentTargetCoordnateIndex+1].Longitude;

        if (IsWithinRange(MyGPS.Latitude, MyGPS.Longitude,targetLat,targetLon, 10.0))
        {
            lineList[CurrentTargetCoordnateIndex].gameObject.SetActive(false); // 도착한 구간 선 비활성화

            // 다음 구간이 아직 남아있다면 인덱스 증가
            if (CurrentTargetCoordnateIndex + 1 < searchPathCoordinate.pathCoordinates.Count - 1)
            {
                CurrentTargetCoordnateIndex++;
            }
            else
            {
                // 마지막 도착점에 도달함
                CurrentTargetCoordnateIndex = 0;
                ClearPathFindUI();
                DestroyPathFindPrefab();
            }
        }
    }

    // 위도/경도를 기반으로 두 점 사이 거리(m) 계산
    public double GetDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371000; // 지구 반지름 (m)
        double dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        double dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);

        double a =
            Mathf.Sin((float)(dLat / 2)) * Mathf.Sin((float)(dLat / 2)) +
            Mathf.Cos((float)(Mathf.Deg2Rad * (float)lat1)) *
            Mathf.Cos((float)(Mathf.Deg2Rad * (float)lat2)) *
            Mathf.Sin((float)(dLon / 2)) * Mathf.Sin((float)(dLon / 2));

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        double distance = R * c;

        return distance; // meter 단위 거리
    }

    public  bool IsWithinRange(double lat1, double lon1, double lat2, double lon2, double radiusMeters)
    {
        double distance = GetDistanceMeters(lat1, lon1, lat2, lon2);
        return distance <= radiusMeters;
    }
    public void ToMyLocation()
    {
        StartCoroutine(SmoothMoveMapToCenter());
    }
    private IEnumerator SmoothMoveMapToCenter()
    {
        
        Vector2 startPosition = mapRawImage.rectTransform.anchoredPosition;
        // mapRawImage의 pivot과 marker의 pivot이 모두 중앙일 때,
        // marker를 화면 중앙(0,0)으로 보내기 위한 map의 anchoredPosition은 -marker.anchoredPosition
        float currentScale = mapRawImage.rectTransform.localScale.x; // x와 y 스케일이                 

        Vector2 targetPosition = -markerRect.anchoredPosition * currentScale;

        float duration = 0.5f; // 이동 시간
        float time = 0;
        while (time < duration)
        {
            mapRawImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            csMapManager.Instance.ClampMapPosition();
            yield return null;
        }
        mapRawImage.rectTransform.anchoredPosition = targetPosition; // 정확한 최종 위치 보정
        csMapManager.Instance.ClampMapPosition();
    }

    // 검색한 장소로 지도 이동
    public void MoveMapToLocation(double lat,double lon,int offset)
    {
        StartCoroutine (MoveMapToLocationSmooth(lat,lon,offset));
    }

    private IEnumerator MoveMapToLocationSmooth(double lat, double lon,int offset)
    {
        ClearSearchLocation();
         // 검색한 장소에 마커찍기
        Vector2 p1 = LatLonToRelativePosition(lat, lon, centerLat, centerLon, zoom);
        Vector2 targetPosition = RelativeToUIPosition(p1, mapRawImage);

        switch(offset)
        {
            // 검색장소
            case 0:
                {
                    if (SearchLocationMarker!=null) Destroy(SearchLocationMarker.gameObject);
                    SearchLocationMarker = Instantiate(SearchLocationMarkerPrefab, mapRawImage.transform);
                    SearchLocationMarker.rectTransform.anchoredPosition = targetPosition;
                    break;
                }
             // 시작장소
            case 1:
                {

                    if (startMarker != null && startMarker.gameObject != null)
                    {
                        Destroy(startMarker.gameObject);
                        startMarker = null;
                    }

                   
                    startMarker = Instantiate(startMarkerPrefab, mapRawImage.transform);
                    startMarker.rectTransform.anchoredPosition = targetPosition;
                    break;
                }
                // 도착장소
            case 2:
                {
                    if (endMarker != null && endMarker.gameObject != null)
                    {
                        Destroy(endMarker.gameObject);
                        endMarker = null;
                    }
                    endMarker = Instantiate(endMarkerPrefab, mapRawImage.transform);
                    endMarker.rectTransform.anchoredPosition = targetPosition;
                    break;
                }
        }
        

        // 검색한 장소를 가운데로 맵 이동
        Vector2 startPosition = mapRawImage.rectTransform.anchoredPosition;

        float currentScale = mapRawImage.rectTransform.localScale.x;

        Vector2 MaptargetPosition = -targetPosition * currentScale;

        float duration = 0.5f; // 이동 시간
        float time = 0;
        while (time < duration)
        {
            mapRawImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, MaptargetPosition, time / duration);
            time += Time.deltaTime;
            csMapManager.Instance.ClampMapPosition();
            yield return null;
        }
        mapRawImage.rectTransform.anchoredPosition = MaptargetPosition; // 정확한 최종 위치 보정
        csMapManager.Instance.ClampMapPosition();
    }

    // 검색장소 리셋
    public void ClearSearchLocation()
    {
        _searchManager.SetSearchScreenButtonUI(false);
        CurrentTargetCoordnateIndex = 0;
        if (SearchLocationMarker) Destroy(SearchLocationMarker.gameObject);
    }

    // 길찾기관련 UI리셋
    public void ClearPathFindUI()
    {
        _searchManager.ClearPathFindUI();
        CurrentTargetCoordnateIndex = 0;
        EsearchStatus = SearchStatus.None;
    }

    // 길찾기 표시 프리펩 삭제
    public void DestroyPathFindPrefab()
    {
        if (startMarker != null && startMarker.gameObject != null)
        {
            Destroy(startMarker.gameObject);
            startMarker = null;
        }

        if (endMarker != null && endMarker.gameObject != null)
        {
            Destroy(endMarker.gameObject);
            endMarker = null;
        }


        foreach (var image in lineList)
        {
            if (image!= null) Destroy(image.gameObject);
        }
    }


}
