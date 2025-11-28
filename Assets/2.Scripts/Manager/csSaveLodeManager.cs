using Data;
using Datainfo;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    private string missionDataPath;
    private string quizDataPath;
    private string stampTourDataPath;


    private void Awake()
    {
        DontDestroyOnLoad(this);
        Initialize();
    }

    public void Initialize()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "gameData.json");
        dataSetPath = Path.Combine(Application.persistentDataPath, "gameSet.json");
        chatHistoryPath = Path.Combine(Application.persistentDataPath, "chatHistory.json");
        savedPlantPath = Path.Combine(Application.persistentDataPath, "savedPlants.json");
        missionDataPath = Path.Combine(Application.persistentDataPath, "missionData.json");
        quizDataPath = Path.Combine(Application.persistentDataPath, "quizData.json");
        stampTourDataPath = Path.Combine(Application.persistentDataPath, "stampTourData.json");

    }

    // ===========================================================
    // SetData 저장/로드
    // ===========================================================
    public void SaveSet()
    {
        SetData data = new SetData();

        data.strPlayerNickName = csSingleton.Instance.strPlayerNickName;
        data.bTermsofUse = csSingleton.Instance.bTermsofUse;
        data.fSoundEffect = csSingleton.Instance.fSoundEffect;
        data.bSoundEffectMute = csSingleton.Instance.bSoundEffectMute;
        data.fBgm = csSingleton.Instance.fBgm;
        data.bBgmMute = csSingleton.Instance.bBgmMute;

        data.fRecommendTimer = csSingleton.Instance.fRecommendTimer;

        data.bAutoLogin = csSingleton.Instance.bAutoLogin;
        data.nSavedLoginType = csSingleton.Instance.nSavedLoginType;
        data.nLanguage = csSingleton.Instance.nLanguage;

        data.UID = csSingleton.Instance.UID;

        data.nPoint = csSingleton.Instance.nPoint;



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

            csSingleton.Instance.strPlayerNickName = data.strPlayerNickName;
            csSingleton.Instance.bTermsofUse = data.bTermsofUse;
            csSingleton.Instance.fSoundEffect = data.fSoundEffect;
            csSingleton.Instance.bSoundEffectMute = data.bSoundEffectMute;
            csSingleton.Instance.fBgm = data.fBgm;
            csSingleton.Instance.bBgmMute = data.bBgmMute;

            csSingleton.Instance.fRecommendTimer = data.fRecommendTimer;

            csSingleton.Instance.bAutoLogin = data.bAutoLogin;
            csSingleton.Instance.nSavedLoginType = data.nSavedLoginType;
            csSingleton.Instance.nLanguage = data.nLanguage;

            csSingleton.Instance.UID = data.UID;

            csSingleton.Instance.nPoint = data.nPoint;

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

    /// <summary>
    /// 미션 저장
    /// </summary>
    public void SaveMission()
    {
        AICreatedMissions data = csSingleton.Instance.savedMissions;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(missionDataPath, json);

        Debug.Log("미션 데이터 저장 완료: " + missionDataPath);
    }

    /// <summary>
    /// 미션 불러오기
    /// </summary>
    public void LoadSavedMission()
    {
        if (File.Exists(missionDataPath))
        {
            string json = File.ReadAllText(missionDataPath);
            AICreatedMissions data = JsonUtility.FromJson<AICreatedMissions>(json);

            csSingleton.Instance.savedMissions = data ?? new AICreatedMissions();

            Debug.Log("미션 데이터 로드 완료: " + missionDataPath);
            
        }
        else
        {
            csSingleton.Instance.savedMissions = new AICreatedMissions();
            SaveMission();
        }
    }


    public void SaveQuizData()
    {
        QuizDataWrapperList data = csSingleton.Instance.savedQuizList;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(quizDataPath, json);

        Debug.Log("퀴즈 저장 완료: " + quizDataPath);
    }

    public void LoadSavedQuiz()
    {
        if (File.Exists(quizDataPath))
        {
            string json = File.ReadAllText(quizDataPath);
            QuizDataWrapperList data = JsonUtility.FromJson<QuizDataWrapperList>(json);

            csSingleton.Instance.savedQuizList = data ?? new QuizDataWrapperList();

            Debug.Log("퀴즈 데이터 로드 완료: " + quizDataPath);

        }
        else
        {
            csSingleton.Instance.savedQuizList = new QuizDataWrapperList();
            SaveQuizData();
        }
    }

    /// <summary>
    /// 미션 저장
    /// </summary>
    public void SaveStampTour()
    {
        StampTourProgressData data = csSingleton.Instance.stampTourProgressData;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(stampTourDataPath, json);

        Debug.Log("미션 데이터 저장 완료: " + stampTourDataPath);
    }

    /// <summary>
    /// 미션 불러오기
    /// </summary>
    public void LoadSavedStampTour()
    {
        if (File.Exists(stampTourDataPath))
        {
            string json = File.ReadAllText(stampTourDataPath);
            StampTourProgressData data = JsonUtility.FromJson<StampTourProgressData>(json);

            csSingleton.Instance.stampTourProgressData = data ?? new StampTourProgressData();

            Debug.Log("스탬프 투어데이터  로드 완료: " + stampTourDataPath);

        }
        else
        {
            csSingleton.Instance.stampTourProgressData = new StampTourProgressData();
            SaveStampTour();
        }
    }

}
