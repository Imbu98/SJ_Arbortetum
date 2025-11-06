using UnityEngine;
using System.Collections.Generic;
using Data;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Windows.Forms;

public class csMissionManager : MonoBehaviour
{
    public static csMissionManager Instance { get { return _Instance; } }
    private static csMissionManager _Instance;

    public csMissonUIManager _missonUIManager;

    // 만들어진 미션 목록 정보 저장
    private AICreatedMissions aiCreatedMissions;

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

    // 몇 번째 생성미션인지 확인하는 인덱스
    [HideInInspector] public int currentMissionIndex = 0;
    // 몇 번째 미션 진행중인지 확인하는 인덱스
    [HideInInspector]public int currentMissionStepIndex = 0;
    // 미션 중인지 확인하는 인덱스
    [HideInInspector]public bool IsMissonOnProgress = false;


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

    // 서버로부터 미션 받아오는 함수
    public async void CreateMisson()
    {
        E_missonStatus = MissionStatus.MissionCreating;
        // 미션 생성중 창으로 변경
        _missonUIManager.ChangeMissonPanel(1);
        // 네트워크에서 await로 미션 생성, 다되면 missonCreated로 변경
        // aiCreatedMissions =  await csNetworkManager.Instance.Get~~
        aiCreatedMissions = CreateTestAIMissions();

        E_missonStatus = MissionStatus.MissonCreated;

        _missonUIManager.ChangeToMission();

    }

    // 미션 생성 취소
    public void CancleCreateMisson()
    {
        // 미션받아오기 전에 전에 취소 누르면 상태바꾸고 취소전환
        E_missonStatus = MissionStatus.None;
        csUI_Manager.Instance.PopupMission(false);
    }

    // 현재 미션목록을 기반으로 미션 목록 UI 생성
    public void SetMissonUI()
    {
            ClearCreatedMissionList();

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

    private void ClearCreatedMissionList()
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
        IsMissonOnProgress = true;

        mission = aiCreatedMissions.missions[missionIndex];

        _missonUIManager.ChangeToMissionStep();

        currentMissionIndex = missionIndex;
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

    //

    public void ClearCurrentMissionStep()
    {
        MissionStep currentMissionDto = aiCreatedMissions.missions[currentMissionIndex].missionStepDetails[currentMissionIndex];

        currentMissionDto.IsCleared = true;

        missionStepList[currentMissionStepIndex].SetClearUI();

        currentMissionStepIndex++;

        // 미션스텝 디테일보다 크면 해당 미션 클리어
        if(currentMissionStepIndex >= mission.missionStepDetails.Count)
        {
            currentMissionStepIndex = 0;

            ClearCurrentMission();
        }
        // 아니면 다음 미션을 진행 UI로 변경
        else
        {
            missionStepList[currentMissionStepIndex].SetProgressUI();
        }
    }
    
    // 현재 진행중이던 미션 클리어
    private void ClearCurrentMission()
    {
        // 다시 플레이 할수 있도록 해당 미션의 미션스탭들 미완료 상태로 전환
        foreach (MissionStep missionStep in mission.missionStepDetails)
        {
            missionStep.IsCleared = false;
        }

        // 미션 클리어 상태로 변경
        aiCreatedMissions.missions[currentMissionIndex].IsCleared = true;

        // 클리어 UI 활성화
        missionList[currentMissionIndex].SetClearUI();

        // 다시 미션 목록창으로 변경
        _missonUIManager.ChangeMissonPanel(2);

        IsMissonOnProgress = false;
    }

    public MissionStep GetCurrentMissionStep()
    {
        return mission.missionStepDetails[currentMissionStepIndex];
    }

    // 현재 미션포기버튼
    // MissionForgiveButton OnClick 연결
    public void ForgiveCurrentMission()
    {
        currentMissionStepIndex = 0;

        if(mission==null)
        {
            Debug.Log("No Mission Data To Forgive");
            return;
        }

        // 각 미션단계의 클리어 여부 초기화
        foreach (MissionStep missionStep in mission.missionStepDetails)
        {
            missionStep.IsCleared = false;
        }


        // 현재 미션 
        mission = null;

        IsMissonOnProgress = false;

        // 미션 포기 후 생성 미션 목록으로 이동
        _missonUIManager.ChangeToMission();
    }
    
    // 만들어진 미션 초기화 ( MissionResetButton Onclick이벤트에 연결)
    public void ResetCreatedMission()
    {

        aiCreatedMissions = null;

        foreach (var mission in missionList)
        {
            if (mission != null)
                Destroy(mission.gameObject);
        }
        missionList.Clear();

        _missonUIManager.ChangeMissonPanel(0);
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
                Description = "가까운 장소를 방문해 주변을 관찰해보세요.",
                IsCleared = false,
                missionDistance = 150,
                missonTimeTaken = 180,
                missionStepDetails = new List<MissionStep>
                {
                    new MissionStep
                    {
                        Description = "나무 쉼터 방문",
                        IsCleared = false,
                        missionDistance = 40,
                        destinationName = "근처 나무 쉼터",
                        plantName = "Tree",
                        destinationCoordinate = new GeoCoordinate(
                            36.496480, 127.283750)
                    },
                    new MissionStep
                    {
                        Description = "벤치 휴식하기",
                        IsCleared = false,
                        missionDistance = 50,
                        destinationName = "작은 벤치",
                        plantName = "Bench",
                        destinationCoordinate = new GeoCoordinate(
                            36.496030, 127.283210)
                    },
                    new MissionStep
                    {
                        Description = "작은 분수대 관찰",
                        IsCleared = false,
                        missionDistance = 30,
                        destinationName = "작은 분수대",
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496300, 127.283400)
                    },
                    new MissionStep
                    {
                        Description = "그늘진 화단 살펴보기",
                        IsCleared = false,
                        missionDistance = 30,
                        destinationName = "그늘 화단",
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
                missionDistance = 140,
                missonTimeTaken = 240,
                missionStepDetails = new List<MissionStep>
                {
                    new MissionStep
                    {
                        Description = "노란 꽃 찾기",
                        IsCleared = false,
                        missionDistance = 40,
                        destinationName = "노란 꽃밭",
                        plantName = "YellowFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.496320, 127.283900)
                    },
                    new MissionStep
                    {
                        Description = "붉은 꽃 관찰",
                        IsCleared = false,
                        missionDistance = 30,
                        destinationName = "레드 블라썸",
                        plantName = "RedFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.495900, 127.283450)
                    },
                    new MissionStep
                    {
                        Description = "보라색 꽃 관찰",
                        IsCleared = false,
                        missionDistance = 35,
                        destinationName = "퍼플 가든",
                        plantName = "PurpleFlower",
                        destinationCoordinate = new GeoCoordinate(
                            36.496050, 127.283300)
                    },
                    new MissionStep
                    {
                        Description = "흰 꽃 구경하기",
                        IsCleared = false,
                        missionDistance = 35,
                        destinationName = "화이트 스타 플라워",
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
                missionDistance = 160,
                missonTimeTaken = 300,
                missionStepDetails = new List<MissionStep>
                {
                    new MissionStep
                    {
                        Description = "산책로 입구 방문",
                        IsCleared = false,
                        missionDistance = 40,
                        destinationName = "산책로 입구",
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496600, 127.283400)
                    },
                    new MissionStep
                    {
                        Description = "전망 포인트 찾아가기",
                        IsCleared = false,
                        missionDistance = 40,
                        destinationName = "전망 좋은 장소",
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.495780, 127.283820)
                    },
                    new MissionStep
                    {
                        Description = "작은 공터에서 휴식",
                        IsCleared = false,
                        missionDistance = 40,
                        destinationName = "작은 공터",
                        plantName = "",
                        destinationCoordinate = new GeoCoordinate(
                            36.496120, 127.283150)
                    },
                    new MissionStep
                    {
                        Description = "하천 근처로 이동",
                        IsCleared = false,
                        missionDistance = 40,
                        destinationName = "작은 하천",
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
