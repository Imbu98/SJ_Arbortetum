using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        string method = "get-route-geojson";

        string methodUrl = url + method;

        //string methodUrl = "http://192.168.0.26:8080/" + method;

        // 요청 데이터 → JSON 문자열 변환
        var body = new
        {
            uid =  csSingleton.Instance.UID,
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
                string json = request.downloadHandler.text;
                

                string responseText = request.downloadHandler.text;
                Debug.Log($"[csNetworkManager] 서버 응답:\n{responseText}");

                try
                {
                    // 2️⃣ JSON 파싱 (route_geometry.coordinates만 추출)
                    JObject data = JObject.Parse(responseText);
                    var coordinatesArray = data["features"][0]["geometry"]["coordinates"];

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

    async public UniTask<SearchPathCoordinate> GetDestinationCoordsAsyncByOsrm(GeoCoordinate startGeoCoordinate, GeoCoordinate EndGeoCoordinate)
    {
        string url = $"https://api.openrouteservice.org/v2/directions/foot-walking?api_key=eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjgyYmU4Zjc5YzNjZTQzZjQ4YzI2MjMxZjkxMGJiOWZmIiwiaCI6Im11cm11cjY0In0=&start={startGeoCoordinate.Longitude},{startGeoCoordinate.Latitude}&end={EndGeoCoordinate.Longitude},{EndGeoCoordinate.Latitude}";
        


        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var operation = req.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OSRM API Error: " + req.error);
                Debug.LogError("[csNetworkManager] 요청 실패!");
                Debug.LogError($"Error: {req.error}");
                Debug.LogError($"ResponseCode: {req.responseCode}");
                Debug.LogError($"DownloadText: {req.downloadHandler.text}");
                return null;
            }
            try
            {

                string json = req.downloadHandler.text;
                JObject data = JObject.Parse(json);

                var geometry = data["features"][0]["geometry"]["coordinates"];
                List<GeoCoordinate> coords = new List<GeoCoordinate>();
                if (geometry != null)
                {
                    foreach (var point in geometry)
                    {
                        float lon = point[0].Value<float>();
                        float lat = point[1].Value<float>();
                        Debug.Log($"Path point: {lat}, {lon}");
                        coords.Add(new GeoCoordinate(lat, lon));
                        // 좌표를 Unity UI 상 위치로 변환해서 선 그리기 가능
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
}
    public async UniTask<string> AsyncGetAIChatResult(ChatMessage userChatMessage)
    {
        string method = "arboretum/api/chat";

        string methodUrl = url + method;

        //string methodUrl = "http://192.168.0.26:8080/" + method;


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
                throw new Exception(request.error);
            }

            string json = request.downloadHandler.text;

            AIChatResponse res = JsonConvert.DeserializeObject<AIChatResponse>(json);

            if (res.data != null)
            {
                string flag = res.data.flag?.ToLower();

                // --- 코스 코드를 수행 ---
                if (flag == "course")
                {
                    if (res.data.data == null || res.data.routeData == null)
                    {
                        Debug.LogError("코스 데이터 없음");
                    }

                    csMissionManager.Instance.CreateMisson(res.data.routeData);
                    Debug.Log("코스 생성 완료");
                }

                else
                {
                    PlantOrPlaceData data = res.data.data;

                    LocationData locationData = new LocationData
                    {
                        koreanName = data.name,
                        englishName = data.name,
                        geoCoordinate = new GeoCoordinate(data.latitude, data.longitude),
                        locationID = 9999
                    };

                    csMapManager.Instance._searchManager.SetSearchUI(locationData, 2);

                    csUIManager.Instance.PopupMap(true);
                }
            }


            return res.response;
        }
    }

    async public UniTask<GetPlantResponse> AsyncGetPlantInfoAsync(Texture2D texture, Action<float> onProgress = null)
    {
        if(texture==null)
        {
            Debug.Log("No Texutre To identify");
        }

        string method = "arboretum/identify-plant";

        string methodUrl = url + method;

        //string methodUrl = "http://192.168.0.26:8080/" + method;

        // Texture2D → PNG
        Texture2D readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        readable.SetPixels(texture.GetPixels());
        readable.Apply();
        byte[] pngBytes = readable.EncodeToPNG();

        // multipart form 생성
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", pngBytes, "plant.png", "image/png");
        form.AddField("organs", "auto");
        form.AddField("uid",csSingleton.Instance.UID);

        using (UnityWebRequest request = UnityWebRequest.Post(methodUrl, form))
        {
            request.SetRequestHeader("Accept", "application/json");

            var op = request.SendWebRequest();

            while (!op.isDone)
            {
                float pct = Mathf.Max(request.uploadProgress, request.downloadProgress);
                onProgress?.Invoke(pct);   // 진행률 텍스트로 전달

                await UniTask.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
                return null;

            return JsonConvert.DeserializeObject<GetPlantResponse>(request.downloadHandler.text);
        }
    }

    //async public UniTask<PlantResponse> AsyncGetPlantInfo(Texture2D texture, Action<float> onProgress = null)
    //{
    //    string method = "generate-plant-info";

    //    string methodUrl = /*url*/ "http://192.168.0.26:8001/" + method;

    //}


    // 관찰하기 성공 시 서버에서 퀴즈 데이터를 받아 로컬에 저장
    public void OnReceiveQuizFromServer(string jsonQuiz)
    {
        // 서버에서 넘어온 단일 퀴즈 데이터 역직렬화
        QuizData newQuiz = JsonConvert.DeserializeObject<QuizData>(jsonQuiz);

        // 퀴즈 추가 (IsFirstCorrect = false 기본)
        QuizDataWrapper quizdataWraaper = new QuizDataWrapper
        {
            quizData = newQuiz,
            IsSolvedQuestion = false
        };

        // 저장
        csSingleton.Instance.savedQuizList.quizDataWrapperList.Add(quizdataWraaper);

        csSaveLodeManager.Instance.SaveQuizData();
    }

    async public UniTask<Texture2D> DownloadImage(string imageUrl)
    {

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Image Load Failed: " + request.error);
            return null;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        return texture;
    }
}