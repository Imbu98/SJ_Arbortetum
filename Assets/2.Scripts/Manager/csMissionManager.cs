using UnityEngine;
using System.Collections.Generic;
using Data;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Windows.Forms;
using System.Collections;

public class csMissionManager : MonoBehaviour
{
    public static csMissionManager Instance { get { return _Instance; } }
    private static csMissionManager _Instance;

    public csMissonUIManager _missonUIManager;

    // 만들어진 미션 목록 정보 저장
    private AICreatedMissions _aiCreatedMissions;

    // AICreatedMissions안의 값이 바뀌면 자동으로 csSingleton에 저장된 값도 바뀌도록 getter/setter 사용
    public AICreatedMissions aiCreatedMissions
    {
        get { return _aiCreatedMissions; }
        set
        {
            _aiCreatedMissions = value;

            if (csSingleton.Instance.savedMissions != null)
                csSingleton.Instance.savedMissions = value;
        }
    }

    // 현재 진행중인 미션 저장
    private Mission mission;

    // 미션 목록으로 만든 UI 부모 트랜스폼
    [SerializeField] private Transform createdMissionHolder;
    // 미션 목록을 만들 UI 프리팹
    [SerializeField]private csCreatedMissonPrefab createdMissionPrefab;
    // 미션 목록 UI 프리팹 저장 리스트
    private List<csCreatedMissonPrefab> missionList = new List<csCreatedMissonPrefab>();

    // 진행 미션 목록으로 만든 UI프리팹 부모 트랜스폼
    [SerializeField] private Transform progressMissonHolder;
    // 진행 미션 목록을 만들 UI 프리팹
    [SerializeField]private csMissionStepPrefab progressMissonPrefab;
    // 미션 목록 UI 프리팹 저장 리스트
    private List<csMissionStepPrefab> missionStepList = new List<csMissionStepPrefab>();


    // 어떤 스타일의 미션을 생성해야 하는지에 대한 변수 ( 일단 쓰지 않을 것 )
    //private int missionStyle = -1;
    // 현재 미션의 생성 상태
    [HideInInspector] public MissionStatus E_missonStatus=MissionStatus.None;

    // 미션 중인지 확인하는 인덱스
    [HideInInspector]public bool IsMissonOnProgress = false;

    // 미션 포기 처리 카운트다운
    private Coroutine missionCountDownRoutine;
    // 포기 처리 시간
    private float missionCountTime= 3000f;




    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        aiCreatedMissions = csSingleton.Instance.savedMissions;

        // missionIndex가 -1이 아니면 미션을 진행중임
        if(aiCreatedMissions.missionIndex!=-1)
        {
            E_missonStatus = MissionStatus.MissonCreated;

            mission = aiCreatedMissions.missions[aiCreatedMissions.missionIndex];

            IsMissonOnProgress = true;

            // 미션이 있으면 UI 만들어놓기
            SetMissonUI();

            //SetMissionStepUI();
        }
        else
        {
            // 생성된 미션 리스트는 있는데 미션을 진행하진 않았던 상태
            if (aiCreatedMissions.missions.Count>0)
            {
                E_missonStatus = MissionStatus.MissonCreated;

                SetMissonUI();
            }
            else
            {
                E_missonStatus = MissionStatus.None;
            }
        }
    }

    public void CreateMisson(AIChatResponse chatResponse)
    {
        //E_missonStatus = MissionStatus.MissionCreating;
        // 미션 생성중 창으로 변경
        //_missonUIManager.ChangeMissonPanel(1);
        // 네트워크에서 await로 미션 생성, 다되면 missonCreated로 변경

        if(IsMissonOnProgress)
        {
            csUIManager.Instance.SetAIChatText("현제진행중인 미션이 있습니다. 미션을 완료하거나 포기한 후에 새로운 미션을 생성할 수 있습니다.");
            return;
        }

        Mission aiCreatedMission = new Mission();

        aiCreatedMission.missionTitle = "AI 추천 식물 관찰 코스";
        aiCreatedMission.missonRewardPoint = 100;
        aiCreatedMission.IsCleared = false;
        aiCreatedMission.Description = "AI가 추천한 코스를 따라가며 식물을 관찰해보세요";

        for (int i =0; i< chatResponse.route.Count;++i)
        {
          MissionStep aicreatedMissionStep =   ConvertRouteToMissionSteps(chatResponse.route[i]);
          aiCreatedMission.missionStepDetails.Add(aicreatedMissionStep);
        }

        aiCreatedMissions.missions.Add(aiCreatedMission);

        aiCreatedMissions.missionIndex = -1;
        aiCreatedMissions.missionStepIndex = -1;

        E_missonStatus = MissionStatus.MissonCreated;

        csUIManager.Instance.PopupMission(true);

        //_missonUIManager.ChangeToMission();

        csSingleton.Instance.savedMissions = aiCreatedMissions;

        csSaveLodeManager.Instance.SaveMission();

    }

    // ai대화에서 나온 루트를 missionStep으로 변환
    public MissionStep ConvertRouteToMissionSteps(SimpleRoute routes)
    {

            MissionStep missionStep = new MissionStep();

        missionStep.plantName = routes.name;
        missionStep.destinationCoordinate =
                new GeoCoordinate(routes.latitude, routes.longitude);
        missionStep.IsCleared = false;
        missionStep.Description = "목적지 주변의 " + routes.name + "을(를) 관찰해보세요.";

        return missionStep;
    }


    // 미션 생성 취소
    public void PopupCancleCreateMisson()
    {
        // 미션받아오기 전에 전에 취소 누르면 상태바꾸고 취소전환
        csPopupPanel.Instance.PopupCancelMission(() =>
        {
            E_missonStatus = MissionStatus.None;
            csUIManager.Instance.PopupMission(false);
        });
    }

    // 현재 미션목록을 기반으로 미션 목록 UI 생성
    public void SetMissonUI()
    {
            ResetCreatedMissionList();

            for (int i=0; i< aiCreatedMissions.missions.Count;++i)
            {
                if(createdMissionPrefab)
                {
                    csCreatedMissonPrefab createdMission = Instantiate(createdMissionPrefab, createdMissionHolder, false);
                    if (createdMission)
                    {
                        createdMission.Init(i, aiCreatedMissions.missions[i]);
                        missionList.Add(createdMission);
                    }
                }
            }
    }

    private void ResetCreatedMissionList()
    {
        foreach (var mission in missionList)
        {
            if (mission != null)
                Destroy(mission.gameObject);
        }
        missionList.Clear();
    }


    public void StartMission(int missionIndex)
    {
        if(missionCountDownRoutine!=null)
        {
            StopCoroutine(missionCountDownRoutine);
        }
        //missionCountDownRoutine = StartCoroutine(MissionCountDownCoroutine());

        IsMissonOnProgress = true;

        mission = aiCreatedMissions.missions[missionIndex];

        aiCreatedMissions.missionIndex = missionIndex;

        aiCreatedMissions.missionStepIndex = 0;

        _missonUIManager.ChangeToMissionStep();

        csSaveLodeManager.Instance.SaveMission();

        csFirebaseLogManager.Instance.Log_Mission(0);

    }

    public void SetMissionStepUI()
    {
        ResetMissionStepList();

        if (mission == null)
        {
            Debug.Log("No Mission Data");
            return;
        }

        for (int i = 0; i < mission.missionStepDetails.Count; ++i)
        {
            if (progressMissonPrefab)
            {
                csMissionStepPrefab progressMission = Instantiate(progressMissonPrefab, progressMissonHolder, false);
                if (progressMission)
                {
                    progressMission.Init(i, mission.missionStepDetails[i]);
                    missionStepList.Add(progressMission);
                }
            }
        }
    }

    private void ResetMissionStepList()
    {
        foreach (var progressMission in missionStepList)
        {
            if (progressMission != null)
                Destroy(progressMission.gameObject);
        }
        missionStepList.Clear();
    }

    // 현재 미션스텝 완료

    public void ClearCurrentMissionStep()
    {
        aiCreatedMissions.missions[aiCreatedMissions.missionIndex].missionStepDetails[aiCreatedMissions.missionStepIndex].IsCleared = true;

        missionStepList[aiCreatedMissions.missionStepIndex].SetClearUI();

        csFirebaseLogManager.Instance.Log_MissionStepClear(aiCreatedMissions.missionStepIndex);

        aiCreatedMissions.missionStepIndex++;

        // 미션스텝 디테일보다 크면 해당 미션 클리어
        if(aiCreatedMissions.missionStepIndex >= mission.missionStepDetails.Count)
        {
            aiCreatedMissions.missionStepIndex = 0;
            ClearCurrentMission();
            _missonUIManager.ChangeToMissionClearPanel();

            _missonUIManager.missionClearButton.onClick.RemoveAllListeners();
            _missonUIManager.missionClearButton.onClick.AddListener(() =>
            {
                csUIManager.Instance.BlockBackButton(false);

                // 다시 미션 목록창으로 변경
                _missonUIManager.ChangeMissonPanel(2);

            });
        }
        // 아니면 다음 미션을 진행 UI로 변경
        else
        {
            missionStepList[aiCreatedMissions.missionStepIndex].SetProgressUI();
        }
        csSaveLodeManager.Instance.SaveMission();
    }
    
    // 현재 진행중이던 미션 클리어
    private void ClearCurrentMission()
    {
        // 다시 플레이 할수 있도록 해당 미션의 미션스탭들 미완료 상태로 전환
        foreach (MissionStep missionStep in mission.missionStepDetails)
        {
            missionStep.IsCleared = false;
        }

        // 미션스텝 정리
        ResetMissionStepList();

        // 클리어했던 미션이 아닐 때만 
        if(aiCreatedMissions.missions[aiCreatedMissions.missionIndex].IsCleared==false)
        {
            // 미션 클리어 상태로 변경
            aiCreatedMissions.missions[aiCreatedMissions.missionIndex].IsCleared = true;

            // 보상 지급
            csSingleton.Instance.RewardPoint(mission.missonRewardPoint);
        }

        // 클리어 UI 활성화
        missionList[aiCreatedMissions.missionIndex].SetClearUI();

        IsMissonOnProgress = false;

        aiCreatedMissions.missionIndex = -1;

        aiCreatedMissions.missionStepIndex = -1;

        csFirebaseLogManager.Instance.Log_Mission(1);

        csSoundManager.Instance.HashPlayEffectSound("5_Mission_Clear");
    }

    

    public MissionStep GetCurrentMissionStep()
    {
        return mission.missionStepDetails[aiCreatedMissions.missionStepIndex];
    }

    // MissionForgiveButton OnClick 연결
    public void PopupForgiveCurrentMission()
    {
        csPopupPanel.Instance.PopupForgiveCurrentMission(ForgiveCurrentMission);
    }

    // 현재 미션포기
    private void ForgiveCurrentMission()
    {
        if (missionCountDownRoutine != null)
        {
            StopCoroutine(missionCountDownRoutine);
        }

        IsMissonOnProgress = false;

        aiCreatedMissions.missionIndex = -1;

        aiCreatedMissions.missionStepIndex = -1;

        if(mission ==null)
        {
            Debug.Log("No Mission Data To Forgive");
            return;
        }

        // 각 미션단계의 클리어 여부 초기화
        foreach (MissionStep missionStep in mission.missionStepDetails)
        {
            missionStep.IsCleared = false;
        }


        // 현재 미션 초기화
        mission = null;

        // 미션 포기 후 생성 미션 목록으로 이동
        _missonUIManager.ChangeToMission();

        csSaveLodeManager.Instance.SaveMission();

        csFirebaseLogManager.Instance.Log_Mission(2);

    }

    // 미션 초기화 팝업 띄우기 ( MissionResetButton Onclick이벤트에 연결)
    public void PopupResetCreatedMission()
    {
        csPopupPanel.Instance.PopupResetMission(ResetCreatedMission);
    }

    // 만들어진 미션 초기화
    private void ResetCreatedMission()
    {
        aiCreatedMissions = new AICreatedMissions();
        aiCreatedMissions.missionIndex = -1;
        aiCreatedMissions.missionStepIndex = -1;

        foreach (var mission in missionList)
        {
            if (mission != null)
                Destroy(mission.gameObject);
        }
        missionList.Clear();

        E_missonStatus = MissionStatus.None;

        csUIManager.Instance.PopupMission(false);

        //_missonUIManager.ChangeMissonPanel(0);

        csSaveLodeManager.Instance.SaveMission();
    }

    private IEnumerator MissionCountDownCoroutine()
    {
        float time = missionCountTime;

        while (time > 0f)
        {

            yield return new WaitForSeconds(1f);
            time--;
        }
        ForgiveCurrentMission();

        yield return null;
    }

    AICreatedMissions CreateTestAIMissions()
    {
        return new AICreatedMissions
        {
            missions = new List<Mission>
        {
            // ✅ 미션 1
            new Mission
            {
                missionTitle = "도심 관찰 미션",
                IsCleared = false,
                missonRewardPoint = 100,
                missionStepDetails = new List<MissionStep>
                {
                    new MissionStep
                    {
                        Description = "나무 쉼터 방문",
                        IsCleared = false,
                        plantName = "Tree",
                        destinationCoordinate = new GeoCoordinate(
                            36.496480, 127.283750)
                    },
                    new MissionStep
                    {
                        Description = "벤치 휴식하기",
                        IsCleared = false,
                        plantName = "Bench",
                        destinationCoordinate = new GeoCoordinate(
                            36.496030, 127.283210)
                    },
                    new MissionStep
                    {
                        Description = "작은 분수대 관찰",
                        IsCleared = false,
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496300, 127.283400)
                    },
                    new MissionStep
                    {
                        Description = "그늘진 화단 살펴보기",
                        IsCleared = false,
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496150, 127.283820)
                    }
                }
            },

            // ✅ 미션 2
            new Mission
            {
                missionTitle = "꽃 찾기 미션",
                Description = "주변의 꽃을 조사해보세요.",
                IsCleared = false,
                 missonRewardPoint = 100,
                missionStepDetails = new List<MissionStep>
                {
                    new MissionStep
                    {
                        Description = "노란 꽃 찾기",
                        IsCleared = false,
                        plantName = "YellowFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.496320, 127.283900)
                    },
                    new MissionStep
                    {
                        Description = "붉은 꽃 관찰",
                        IsCleared = false,
                        plantName = "RedFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.495900, 127.283450)
                    },
                    new MissionStep
                    {
                        Description = "보라색 꽃 관찰",
                        IsCleared = false,
                        plantName = "PurpleFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.496050, 127.283300)
                    },
                    new MissionStep
                    {
                        Description = "흰 꽃 구경하기",
                        IsCleared = false,
                        plantName = "WhiteFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.496420, 127.283600)
                    }
                }
            },

            // ✅ 미션 3
            new Mission
            {
                missionTitle = "동네 탐험 미션",
                Description = "근처를 걸으며 탐험해봅시다.",
                IsCleared = false,
                 missonRewardPoint = 100,
                missionStepDetails = new List<MissionStep>
                {
                    new MissionStep
                    {
                        Description = "산책로 입구 방문",
                        IsCleared = false,
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496600, 127.283400)
                    },
                    new MissionStep
                    {
                        Description = "전망 포인트 찾아가기",
                        IsCleared = false,
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.495780, 127.283820)
                    },
                    new MissionStep
                    {
                        Description = "작은 공터에서 휴식",
                        IsCleared = false,
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496120, 127.283150)
                    },
                    new MissionStep
                    {
                        Description = "하천 근처로 이동",
                        IsCleared = false,
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496050, 127.283980)
                    }
                }
            }
        }
        };
    }
}
