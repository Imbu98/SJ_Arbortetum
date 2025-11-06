using Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class csSingleton : MonoBehaviour
{
    public static csSingleton Instance { get { return _Instance; } }
    private static csSingleton _Instance;

    // 수목원 내의 장소 정보를 담고있는 리스트
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
                Debug.LogWarning($"⚠️ CSV 파싱 실패: {lines[i]}");
            }
        }
    }


    /// <summary>
    /// 현재 언어에 맞는 이름으로 검색
    /// </summary>
    public List<LocationData> Search(string keyword, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<LocationData>(); // 🔹 비어 있으면 바로 반환

        keyword = keyword.Trim().ToLower();

        return AllLocations.FindAll(data =>
            (languageCode == "ko" && data.koreanName.Contains(keyword)) ||
            (languageCode != "ko" && data.englishName.ToLower().Contains(keyword))
        );
    }

}
