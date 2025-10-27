using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Networking;
using UnityEngine.Timeline;
using UnityEngine.UI;
using System;

public class csMapManager : MonoBehaviour
{
    public static csMapManager Instance { get { return _Instance; } }
    private static csMapManager _Instance;

    // 맵 이미지
    public RawImage mapRawImage;

    // 각 좌표 사이를 선으로 잇기 위한프리펩
    [SerializeField] private Image linePrefab;
    private float lineSize = 25f;

    // GPS 좌표 리스트
    public List<Vector2> gpsList = new List<Vector2>();

    // 구글 API 지도 설정 관련 변수
    [Header("Map SET")]
    public string strBaseURL = "";
    public int originZoom;
    public int mapWidth;
    public int mapHeight;
    public string strAPIKey = "";

    // 사용자의 현재 위도와 경도 가져오기 위한 변수
    public GPS MyGPS;
    private double latitude;
    private double longitude;
    public double save_latitude;
    public double save_longitude;

    public int currentZoom;

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
        currentZoom = originZoom;

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

        LoadMap(originZoom);
        save_latitude = MyGPS.Latitude;
        save_longitude = MyGPS.Longitude;
    }

    // 구글 API로 지도 불러오는 함수
    public async void LoadMap(int zoomAmount,double centerLat=-1, double centerLong=-1)
    {
        mapRawImage.rectTransform.anchoredPosition = Vector2.zero;

        // LoadMap시 좌표값이 들어오면 해당 좌표로, 안들어오면 GPS 좌표(내 좌표)로 설정
        if (centerLat != -1 && centerLong != -1)
        {
            latitude = centerLat;
            longitude = centerLong;
        }
        else
        {
            latitude = MyGPS.Latitude;
            longitude = MyGPS.Longitude;
        }
            

        mapRawImage.rectTransform.sizeDelta = new Vector2(mapWidth, mapHeight);
        //mapWidth = Screen.width;
        //mapHeight = Screen.height;

        string url = strBaseURL + "center=" + latitude + "," + longitude +
            "&zoom=" + zoomAmount.ToString() +
            "&size=" + mapWidth.ToString() + "x" + mapHeight.ToString()
            + "&key=" + strAPIKey;

        Debug.Log("URL : " + url);

        url = UnityWebRequest.UnEscapeURL(url);
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);

        await req.SendWebRequest(); //req값 반환

        mapRawImage.texture = DownloadHandlerTexture.GetContent(req); // 맵 >> 이미지에 적용
    }

    public void OnPathButtonPressed()
    {
        double centerLat = MyGPS.Latitude;
        double centerLon = MyGPS.Longitude;
        StartCoroutine(DrawPathAnimated(gpsList, centerLat, centerLon));
    }

    public void SearchPath(List<Vector2> coords)
    {
        double centerLat = MyGPS.Latitude;
        double centerLon = MyGPS.Longitude;
        StartCoroutine(DrawPathAnimated(coords, centerLat, centerLon));
    }

    // AI에서 받아온 좌표마다 이어주는 함수
    IEnumerator DrawPathAnimated(List<Vector2> coords, double centerLat, double centerLon)
    {
        for (int i = 0; i < coords.Count - 1; i++)
        {
            Vector2 p1 = LatLonToRelativePosition(coords[i].x, coords[i].y, centerLat, centerLon, originZoom);
            Vector2 p2 = LatLonToRelativePosition(coords[i + 1].x, coords[i + 1].y, centerLat, centerLon, originZoom);

            Vector2 startUI = RelativeToUIPosition(p1, mapRawImage);
            Vector2 endUI = RelativeToUIPosition(p2, mapRawImage);

            Vector2 dir = endUI - startUI;
            float distance = dir.magnitude;

            Image line = Instantiate(linePrefab, mapRawImage.transform);
            RectTransform rect = line.rectTransform;
            rect.anchoredPosition = startUI;
            rect.sizeDelta = new Vector2(distance, 10f);
            rect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

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

        Vector2 LatLonToRelativePosition(double lat, double lon, double centerLat, double centerLon, int zoom)
        {
            // 위경도를 '구글 타일 좌표'로 변환
            Vector2 centerPixel = LatLonToPixel(centerLat, centerLon, zoom);
            Vector2 pointPixel = LatLonToPixel(lat, lon, zoom);

            // 중심 대비 상대 좌표
            Vector2 delta = pointPixel - centerPixel;
            return delta;
        }

        Vector2 RelativeToUIPosition(Vector2 relative, RawImage mapImage)
        {
            RectTransform rect = mapImage.rectTransform;
            float scaleX = rect.rect.width / (float)mapImage.texture.width;   // 512는 StaticMap의 기본 타일 크기
            float scaleY = rect.rect.height / (float)mapImage.texture.height;

            Vector2 uiPos = new Vector2(relative.x * scaleX, -relative.y * scaleY);
            return uiPos;
        }

        // 구글 지도에서 위도/경도를 픽셀 좌표로 변환하는 함수
        public Vector2 LatLonToPixel(double lat, double lon, int zoom)
        {
            double siny = Mathf.Sin((float)(lat * Mathf.Deg2Rad));
            siny = Mathf.Clamp((float)siny, -0.9999f, 0.9999f);

            double x = 256 * (0.5 + lon / 360);
            double y = 256 * (0.5 - Mathf.Log((float)((1 + siny) / (1 - siny))) / (4 * Mathf.PI));

            double scale = Mathf.Pow(2, zoom);
            return new Vector2((float)(x * scale), (float)(y * scale));
        }
   
}
