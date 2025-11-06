using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class csLocalizationManager : MonoBehaviour
{
    public static csLocalizationManager Instance { get { return _Instance; } }
    private static csLocalizationManager _Instance;

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

    // Localization 테이블 이름 (필요하면 바꾸기)
    private const string TableName = "LanguageTable";

    /// <summary>
    /// Localization String Table에서 key를 찾아 현재 언어의 string을 리턴한다.
    /// </summary>
    public string LocalizationString(string tableKey)
    {
        // StringTable 가져오기
        StringTable table = LocalizationSettings.StringDatabase.GetTable(TableName);

        if (table == null)
        {
            Debug.LogError($"[Localization] String Table '{TableName}'을 찾을 수 없습니다.");
            return tableKey; // fallback
        }

        // Entry 찾기
        StringTableEntry entry = table.GetEntry(tableKey);

        if (entry == null)
        {
            Debug.LogWarning($"[Localization] Key '{tableKey}'가 StringTable '{TableName}'에 존재하지 않습니다.");
            return tableKey; // fallback
        }

        // 현재 언어 텍스트 반환
        return entry.LocalizedValue;
    }
}
