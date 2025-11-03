using Data;
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
        // 요청 JSON 구성
        string jsonBody = $@"
        {{
            ""waypoints"": [
                {{ ""lat"": {startGeoCoordinate.Latitude}, ""lon"": {startGeoCoordinate.Longitude} }},
                {{ ""lat"": {EndGeoCoordinate.Latitude}, ""lon"": {EndGeoCoordinate.Longitude} }}
            ]
        }}";

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
                    // 응답 JSON → RouteResponseDto
                    var routeResponse = JsonUtility.FromJson<SearchPathCoordinate>(responseText);

                    // 변환 후 반환
                    Data.SearchPathCoordinate result = new Data.SearchPathCoordinate
                    {
                        pathCoordinates = routeResponse.pathCoordinates
                    };

                    Debug.Log($"[csNetworkManager] 경로 좌표 {result.pathCoordinates.Count}개 수신 완료");
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