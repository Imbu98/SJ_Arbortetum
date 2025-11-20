using Firebase.Analytics; // Firebase 애널리틱스 네임스페이스 추가
using UnityEngine;
using Firebase;

/// <summary>
/// 뒤끝(The Backend) 로그를 Google Analytics for Firebase로 전송하도록 변환한 클래스입니다.
/// </summary>
public class csFirebaseLogManager : MonoBehaviour
{
    private static csFirebaseLogManager instance;
    public static csFirebaseLogManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindFirstObjectByType<csFirebaseLogManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("FirebaseLogManager").AddComponent<csFirebaseLogManager>();
                    instance = newSingleton;
                }
            }
            return instance;

        }
        private set
        {
            instance = value;
        }

    }

    private void Awake()
    {
        var objs = FindObjectsByType<csFirebaseLogManager>(FindObjectsSortMode.None);

        if (objs.Length != 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);



    }

    private void Start()
    {
        // 구글애널리틱스를 위한 파이어베이스앱초기화
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                // Firebase가 성공적으로 준비되었습니다.
                // 이 시점부터 자동 수집이 시작됩니다.
                Debug.Log("Firebase is initialized and ready.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public void Log_Mission(int index)
    {
        switch(index)
        {
            // 미션 시작 로그 ( 0: 시작,1:완료,2:포기)
            case 0:
                {
                    FirebaseAnalytics.LogEvent("Mission",
                                new Parameter("MissionLog", "missionStartCount")
                            );
                    Debug.Log($"GA Log Sent:MissionStart)");
                    break;
                }
                case 1:
                {
                    FirebaseAnalytics.LogEvent("Mission",
            new Parameter("MissionLog", "missionClearCount")
        );

                    Debug.Log($"GA Log Sent:MissionClear)");
                    break;
                }
            case 2:
                {
                    FirebaseAnalytics.LogEvent("Mission",
           new Parameter("MissionLog", "missionForgiveCount")
       );
                    Debug.Log($"GA Log Sent:MissionClear)");
                    break;
                }
        }
    }

    public void Log_MissionStepClear(int stepindex)
    {
        FirebaseAnalytics.LogEvent("Mission",
            new Parameter("MissionStepClear", $"mission_{stepindex}")
        );

        Debug.Log($"GA Log Sent:MissionStepClear)");
    }

    public void Log_StartPathFind()
    {
        FirebaseAnalytics.LogEvent("PathFind",
            new Parameter("PathFindStart","pathFind")
        );

        Debug.Log($"GA Log Sent:MissionStepClear)");
    }

    public void Log_ChatWithAI()
    {
        FirebaseAnalytics.LogEvent("AIChat",
            new Parameter("AIChatStart", "aiChat")
        );

        Debug.Log($"GA Log Sent:MissionStepClear)");
    }
}
