using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Networking;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class csMapManager : MonoBehaviour
{
    public SearchPathCoordinate currentGeoCoordinate; // 현재 길찾기 좌표
    private int CurrentTargetCoordnateIndex; // 현재 목적지 좌표 인덱스 ( 0에는 내 위치, 목적지는 1부터 시작)
    private bool IsPathSearching = false;

    public static csMapManager Instance { get { return _Instance; } }
    private static csMapManager _Instance;

    // 맵 이미지
    public RawImage mapRawImage;

    // 길 찾기
    [SerializeField] private Image linePrefab;// 각 좌표 사이를 선으로 잇기 위한프리펩
    [SerializeField] private List<Image> lineList;
    private float lineSize = 20f;
    [SerializeField] private List<Color> lineColors;


    // 에디터 테스트용 GPS 좌표 리스트
    public List<GeoCoordinate> gpsList = new List<GeoCoordinate>();

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
        UpdateMarkerOnly();
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
        CheckOnArrive();
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

    public void OnPathButtonPressed()
    {
        //double centerLat = MyGPS.Latitude;
        //double centerLon = MyGPS.Longitude;
        gpsList.Insert(0,new GeoCoordinate(MyGPS.Latitude, MyGPS.Longitude));
        currentGeoCoordinate = new SearchPathCoordinate();
        currentGeoCoordinate.pathCoordinates = gpsList;
        StartCoroutine(DrawPathAnimated(currentGeoCoordinate.pathCoordinates, centerLat, centerLon));
        SetCoordinate(0);
    }

    public void SearchPath(List<GeoCoordinate> coords)
    {
        //double centerLat = MyGPS.Latitude;
        //double centerLon = MyGPS.Longitude;
        StartCoroutine(DrawPathAnimated(coords, centerLat, centerLon));
    }

    // AI에서 받아온 좌표마다 이어주는 함수
    IEnumerator DrawPathAnimated(List<GeoCoordinate> coords, double centerLat, double centerLon)
    {
        if (coords == null || coords.Count < 2)
            yield break;

        // 길찾기를 요청하면 기존의 선 제거
        foreach (var lineImage in lineList)
        {
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

            line.color = lineColors[i];
            RectTransform rect = line.rectTransform;
            rect.anchoredPosition = startUI;
            rect.sizeDelta = new Vector2(distance, lineSize);
            rect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            // 선이 그려지는 애니메이션
            line.fillAmount = 0;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime;
                line.fillAmount = t;
                yield return null;
            }
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

#if UNITY_IOS || UNITY_ANDROID
        //var location = Input.location.lastData;
        //double lat = location.latitude;
        //double lon = location.longitude;
#endif
#if UNITY_EDITOR
        double lat = MyGPS.Latitude;
        double lon = MyGPS.Longitude;
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
        simulatedHeading += Time.deltaTime * 30f; // 초당 30도 회전
        if (simulatedHeading > 360) simulatedHeading -= 360;
        arrowRect.localRotation = Quaternion.Euler(0, 0, -simulatedHeading);

        //float heading = Input.compass.trueHeading;
        //// 기본: Z축 기준 회전
        //arrowRect.localRotation = Quaternion.Euler(0, 0, -heading);
    }

    private void CheckOnArrive()
    {
        // 목적지 도착 체크 로직
        if(currentGeoCoordinate == null || currentGeoCoordinate.pathCoordinates.Count == 0)
            return;

        double targetLat = currentGeoCoordinate.pathCoordinates[CurrentTargetCoordnateIndex+1].Latitude;
        double targetLon = currentGeoCoordinate.pathCoordinates[CurrentTargetCoordnateIndex+1].Longitude;

        if (IsWithinRange(MyGPS.Latitude, MyGPS.Longitude,targetLat,targetLon, 10.0))
        {
            // 일단도착하면 미션 시작 알림, 혹은 도착 알림 (지금은 테스트 용으로 도착하면 첫번째 라인프리펩을 회색처리해주자)
            Color color = lineList[CurrentTargetCoordnateIndex].color;
            color.a = 0.5f;
            color = Color.gray;
            lineList[CurrentTargetCoordnateIndex].color = color;


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

    // 전체 길찾기 좌표 중에서 현재 목표 좌표 설정
    public void SetCoordinate(int index)
    {
        CurrentTargetCoordnateIndex = index; // 첫번째는 내 위치이므로 목표 좌표는 +1
        IsPathSearching = true;

        // 활성화된 라인은 알파값을 1로 설정
        Color color = lineList[index].color;
        color.a = 1f;
        lineList[index].color = color;
    }
    public void ToMyLocation()
    {
        StartCoroutine(SmoothMoveMapToCenter());
    }
    private IEnumerator SmoothMoveMapToCenter()
    {
        float duration = 0.5f; // 이동 시간
        Vector2 startPosition = mapRawImage.rectTransform.anchoredPosition;
        // mapRawImage의 pivot과 marker의 pivot이 모두 중앙일 때,
        // marker를 화면 중앙(0,0)으로 보내기 위한 map의 anchoredPosition은 -marker.anchoredPosition 입니다.
        float currentScale = mapRawImage.rectTransform.localScale.x; // x와 y 스케일이                 

        Vector2 targetPosition = -markerRect.anchoredPosition * currentScale;


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
}
