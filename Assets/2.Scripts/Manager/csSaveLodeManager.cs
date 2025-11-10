using Datainfo;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Data;

public class csSaveLodeManager : MonoBehaviour
{
    private static csSaveLodeManager _instance;
    public static csSaveLodeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("SaveLoadManager");
                _instance = obj.AddComponent<csSaveLodeManager>();
            }
            return _instance;
        }
    }

    private string dataPath;
    private string dataSetPath;
    private string chatHistoryPath;
    private string savedPlantPath;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Initialize();
        InitializeSetting();
    }

    public void Initialize()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "gameData.json");
    }

    public void InitializeSetting()
    {
        dataSetPath = Path.Combine(Application.persistentDataPath, "gameSet.json");
        chatHistoryPath = Path.Combine(Application.persistentDataPath, "chatHistory.json");
        savedPlantPath = Path.Combine(Application.persistentDataPath, "savedPlants.json");
    }
        

    // ===========================================================
    // GameData 저장/로드
    // ===========================================================
    public void SaveData()
    {
        GameData data = new GameData();
        data.strPlayerNickName = csSingleton.Instance.strPlayerNickName;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(dataPath, json);
        Debug.Log("게임 데이터 저장 완료: " + dataPath);
    }

    public void Load()
    {
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            csSingleton.Instance.strPlayerNickName = data.strPlayerNickName;
        }
        else
        {
            Debug.Log("초기 데이터 생성");
            SaveData();
        }
    }

    // ===========================================================
    // SetData 저장/로드
    // ===========================================================
    public void SaveSet()
    {
        SetData data = new SetData();

        data.bTermsofUse = csSingleton.Instance.bTermsofUse;
        data.fSoundEffect = csSingleton.Instance.fSoundEffect;
        data.nSoundEffectMute = csSingleton.Instance.nSoundEffectMute;
        data.fBgm = csSingleton.Instance.fBgm;
        data.nBgmMute = csSingleton.Instance.nBgmMute;

        data.bAutoLogin = csSingleton.Instance.bAutoLogin;
        data.nSavedLoginType = csSingleton.Instance.nSavedLoginType;
        data.nLanguage = csSingleton.Instance.nLanguage;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(dataSetPath, json);

        Debug.Log("설정 데이터 저장 완료: " + dataSetPath);
    }


    public void LoadSet()
    {
        if (File.Exists(dataSetPath))
        {
            string json = File.ReadAllText(dataSetPath);
            SetData data = JsonUtility.FromJson<SetData>(json);

            csSingleton.Instance.bTermsofUse = data.bTermsofUse;
            csSingleton.Instance.fSoundEffect = data.fSoundEffect;
            csSingleton.Instance.nSoundEffectMute = data.nSoundEffectMute;
            csSingleton.Instance.fBgm = data.fBgm;
            csSingleton.Instance.nBgmMute = data.nBgmMute;

            csSingleton.Instance.bAutoLogin = data.bAutoLogin;
            csSingleton.Instance.nSavedLoginType = data.nSavedLoginType;
            csSingleton.Instance.nLanguage = data.nLanguage;

        }
        else
        {
            Debug.Log("초기 설정 생성");
            SaveSet();
        }
    }
    public void SaveChatHistory()
    {
        string json = JsonUtility.ToJson(new ChatHistoryWrapper
        {
            chatList = csSingleton.Instance.strSavedChatHistory
        }, true);

        File.WriteAllText(chatHistoryPath, json);
        Debug.Log("채팅 기록 저장 완료: " + chatHistoryPath);
    }

    public void LoadChatHistory()
    {
        if (File.Exists(chatHistoryPath))
        {
            string json = File.ReadAllText(chatHistoryPath);
            ChatHistoryWrapper data = JsonUtility.FromJson<ChatHistoryWrapper>(json);

            csSingleton.Instance.strSavedChatHistory = data.chatList ?? new List<ChatMessage>();
        }
        else
        {
            csSingleton.Instance.strSavedChatHistory = new List<ChatMessage>();
            SaveChatHistory();
        }
    }

    public void SaveSavedPlant()
    {
        SavedPlantWrapper wrapper = new SavedPlantWrapper
        {
            plantList = new List<string>(csSingleton.Instance.savedPlant)
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savedPlantPath, json);
    }

    public void LoadSavedPlant()
    {
        if (File.Exists(savedPlantPath))
        {
            string json = File.ReadAllText(savedPlantPath);
            SavedPlantWrapper wrapper = JsonUtility.FromJson<SavedPlantWrapper>(json);

            csSingleton.Instance.savedPlant =
                wrapper.plantList != null ? new HashSet<string>(wrapper.plantList) : new HashSet<string>();
        }
        else
        {
            csSingleton.Instance.savedPlant = new HashSet<string>();
            SaveSavedPlant();
        }
    }
}
