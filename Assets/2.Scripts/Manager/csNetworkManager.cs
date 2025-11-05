using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class csNetworkManager : MonoBehaviour
{
    public static csNetworkManager Instance { get { return _Instance; } }
    private static csNetworkManager _Instance;

    [SerializeField] private string url = "http://192.168.0.26:8000/find-optimal-route";

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

    /// <summary>
    /// 서버에 시작/도착 좌표를 보내고, 최적 경로 좌표 리스트를 반환합니다.
    /// </summary>
    public async Task<SearchPathCoordinate> GetDestinationCoordsAsync(GeoCoordinate startGeoCoordinate, GeoCoordinate EndGeoCoordinate)
    {
        // 요청 데이터 → JSON 문자열 변환
        var body = new
        {
            waypoints = new[]
            {
                new { lat = startGeoCoordinate.Latitude, lon = startGeoCoordinate.Longitude },
                new { lat = EndGeoCoordinate.Latitude, lon = EndGeoCoordinate.Longitude }
            }
        };

        string jsonBody = JsonConvert.SerializeObject(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);


        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[csNetworkManager] 요청 전송 중...\n{jsonBody}");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[csNetworkManager] 서버 응답:\n{responseText}");

                try
                {
                    // 2️⃣ JSON 파싱 (route_geometry.coordinates만 추출)
                    JObject json = JObject.Parse(responseText);
                    JArray coordinatesArray = (JArray)json["route_geometry"]?["coordinates"];

                    // 3️⃣ GeoCoordinate 리스트로 변환
                    List<GeoCoordinate> coords = new List<GeoCoordinate>();
                    if (coordinatesArray != null)
                    {
                        foreach (var point in coordinatesArray)
                        {
                            // 나중에 서버에서 순서 바꾸면 바뀌도록 
                            double lon = point[0].Value<double>();
                            double lat = point[1].Value<double>();
                            coords.Add(new GeoCoordinate(lat, lon));
                        }
                    }

                    // 4️⃣ 최종 반환 객체
                    SearchPathCoordinate result = new SearchPathCoordinate
                    {
                        pathCoordinates = coords
                    };

                    Debug.Log($"[csNetworkManager] 경로 좌표 {coords.Count}개 수신 완료 ✅");
                    return result;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[csNetworkManager] JSON 파싱 실패: {e.Message}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"[csNetworkManager] 요청 실패: {request.error}");
                return null;
            }
        }
    }
}