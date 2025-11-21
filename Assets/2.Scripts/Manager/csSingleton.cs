using Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Localization.Settings;

public class csSingleton : MonoBehaviour
{
    public static csSingleton Instance { get { return _Instance; } }
    private static csSingleton _Instance;

    [HideInInspector]public string strPlayerNickName; // 나중에 private로 설정

    [HideInInspector] public bool bTermsofUse=false;//이용약관 동의

    [HideInInspector] public float fBgm;
    [HideInInspector] public float fSoundEffect;
    [HideInInspector] public bool bBgmMute;// 배경음 on/off
    [HideInInspector] public bool bSoundEffectMute;//효과음 on/off

    [HideInInspector] public float fRecommendTimer;

    [HideInInspector] public bool bAutoLogin = false;//자동 로그인

    [HideInInspector] public int nSavedLoginType = 0;//로그인 타입 1: 구글, 2: 애플

    [HideInInspector] public int nLanguage = 0;//언어설정

    [HideInInspector] public string UID; // 유저 고유 아이디

    [HideInInspector] public List<ChatMessage> strSavedChatHistory; // 채팅 기록 

    [HideInInspector] public HashSet<string> savedPlant = new HashSet<string>(); // 관찰한 식물 

    [HideInInspector] public AICreatedMissions savedMissions; // 미션 저장

    [HideInInspector] public int nPoint; // 미션 포인트(재화)



    // CSV파일을 읽어 수목원 내의 장소 정보를 담고있는 리스트
    public List<LocationData> AllLocations = new List<LocationData>();

    // 언어 코드
    public string languageCode = "ko";

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadAllLocation();
        }
        else
        {
            Destroy(this.gameObject);
        }

        // 나중에 언어설정 생기면 해당 코드를 같이 넣어주기
        var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
        LocalizationSettings.SelectedLocale = locale;
    }

    // csv의 모든 장소 넣어두기 
    private void LoadAllLocation()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CSV/AllLocations");
        if (csvFile == null)
        {
            Debug.LogError("❌ AllLocations.csv 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Replace("\r\n", "\n").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = lines[i].Split(',');
            if (cols.Length < 5) continue; // 최소 5열 필요

            string ko = cols[0].Trim();
            string en = cols[1].Trim();

            if (double.TryParse(cols[2].Trim(), out double lat) &&
                double.TryParse(cols[3].Trim(), out double lon) &&
                int.TryParse(cols[4].Trim(), out int id))
            {
                AllLocations.Add(new LocationData(ko, en, lat, lon, id));
            }
            else
            {
                Debug.LogWarning($" CSV 파싱 실패: {lines[i]}");
            }
        }
    }


    /// <summary>
    /// 현재 언어에 맞는 이름으로 검색
    /// </summary>
    public List<LocationData> Search(string keyword, string languageCode)
    {
        //  Empty 검색
        if (string.IsNullOrWhiteSpace(keyword))
            return _emptyList;  // static readonly List<LocationData>()

        keyword = keyword.Trim();
        string keywordLower = keyword.ToLower();

        //  현재 내 위치
        double myLat = csMapManager.Instance.MyGPS.Latitude;
        double myLon = csMapManager.Instance.MyGPS.Longitude;

        bool isKorean = languageCode == "ko";

        //  같은 이름 중 가장 가까운 장소만 저장
        Dictionary<string, (LocationData data, double dist)> bestMap =
            new Dictionary<string, (LocationData, double)>(64);

        for (int i = 0; i < AllLocations.Count; i++)
        {
            var loc = AllLocations[i];

            //  언어에 맞는 이름 선택
            string name = isKorean ? loc.koreanName : loc.englishName;

            //  keyword 필터
            if (!name.ToLower().Contains(keywordLower))
                continue;

            //  거리 계산
            double dist = csMapManager.Instance.GetDistanceMeters(csMapManager.Instance.GetMyGPS(), loc.geoCoordinate);

            //  처음 등장한 이름이면 바로 저장
            if (!bestMap.TryGetValue(name, out var saved))
            {
                bestMap[name] = (loc, dist);
                continue;
            }

            //  이미 존재하면 더 가까운 것만 유지
            if (dist < saved.dist)
            {
                bestMap[name] = (loc, dist);
            }
        }

        //  최종 리스트 변환 (딱 한 번만 new)
        List<LocationData> result = new List<LocationData>(bestMap.Count);
        foreach (var kv in bestMap)
            result.Add(kv.Value.data);

        return result;
    }

    //  GC 최소화를 위한 빈 리스트 패턴
    private static readonly List<LocationData> _emptyList = new List<LocationData>(0);

    public void RewardPoint(int reward)
    {
        nPoint += reward;

        csSaveLodeManager.Instance.SaveSet();

        csUIManager.Instance.mainScreen.UpdatePointUI();
    }

}
