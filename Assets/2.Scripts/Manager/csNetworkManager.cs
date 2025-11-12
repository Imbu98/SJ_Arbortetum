using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static System.Net.WebRequestMethods;

public class csNetworkManager : MonoBehaviour
{
    public static csNetworkManager Instance { get { return _Instance; } }
    private static csNetworkManager _Instance;

    private string url = "http://3.35.86.123/";

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
    async public UniTask<SearchPathCoordinate> GetDestinationCoordsAsync(GeoCoordinate startGeoCoordinate, GeoCoordinate EndGeoCoordinate)
    {
        string method = "find-optimal-route";

        string methodUrl = url+ method;
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


        using (UnityWebRequest request = new UnityWebRequest(methodUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");     // ✅ Swagger에서 자동 추가됨
            request.SetRequestHeader("Authorization", "Bearer xxxx");   // ✅ 토큰 필요하면

            Debug.Log($"[csNetworkManager] 요청 URL ...\n{request.url}");


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
                    else
                    {
                        Debug.Log("[csNetworkManager] coordinatesArray is Null");
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
                Debug.LogError("[csNetworkManager] 요청 실패!");
                Debug.LogError($"Error: {request.error}");
                Debug.LogError($"ResponseCode: {request.responseCode}");
                Debug.LogError($"DownloadText: {request.downloadHandler.text}");
                return null;
            }
        }
    }

    public async UniTask<string> AsyncGetAIChatResult(ChatMessage userChatMessage)
    {
        string method = "arboretum/api/chat";

        string methodUrl = url + method;

        string jsonData = JsonConvert.SerializeObject(userChatMessage);

        using (UnityWebRequest request = new UnityWebRequest(methodUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AI 서버 요청 실패: " + request.error);
                return null;
            }

            string json = request.downloadHandler.text;

            AIChatResponse res = JsonConvert.DeserializeObject<AIChatResponse>(json);

            return res.response;
        }
    }

    async public UniTask<PlantResponse> AsyncGetPlantImageAsync(Texture2D texture, Action<float> onProgress = null)
    {
        if(texture==null)
        {
            Debug.Log("No Texutre To identify");
        }

        string method = "arboretum/identify-plant";

        string methodUrl = url + method;

        // Texture2D → PNG
        Texture2D readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        readable.SetPixels(texture.GetPixels());
        readable.Apply();
        byte[] pngBytes = readable.EncodeToPNG();

        // multipart form 생성
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", pngBytes, "plant.png", "image/png");
        form.AddField("organs", "auto");

        using (UnityWebRequest request = UnityWebRequest.Post(methodUrl, form))
        {
            request.SetRequestHeader("Accept", "application/json");

            var op = request.SendWebRequest();

            while (!op.isDone)
            {
                float pct = Mathf.Max(request.uploadProgress, request.downloadProgress);
                onProgress?.Invoke(pct);   // ✅ 진행률 텍스트로 전달

                await UniTask.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
                return null;

            return JsonConvert.DeserializeObject<PlantResponse>(request.downloadHandler.text);
        }
    }

    //async public UniTask<PlantResponse> AsyncGetPlantInfo(Texture2D texture, Action<float> onProgress = null)
    //{
    //    string method = "generate-plant-info";

    //    string methodUrl = /*url*/ "http://192.168.0.26:8001/" + method;

    //}
}