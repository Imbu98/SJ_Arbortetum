using UnityEngine;
using System.Collections.Generic;
using Data;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;

public class csMissionManager : MonoBehaviour
{
    public static csMissionManager Instance { get { return _Instance; } }
    private static csMissionManager _Instance;

    public csMissonUIManager _missonUIManager;

    // 만들어진 미션 목록 정보 저장
    private AICreatedMissions aiCreatedMissions;

    // 현재 진행중인 미션 저장
    private Mission missionContainer;

    // 미션 목록으로 만든 UI 부모 트랜스폼
    private Transform createdMissionHolder;
    // 미션 목록을 만들 UI 프리팹
    private csCreatedMissonPrefab createdMissionPrefab;
    // 미션 목록 UI 프리팹 저장 리스트
    private List<csCreatedMissonPrefab> createdMissionList = new List<csCreatedMissonPrefab>();

    // 진행 미션 목록으로 만든 UI프리팹 부모 트랜스폼
    private Transform progressMissonHolder;
    // 진행 미션 목록을 만들 UI 프리팹
    private csProgressMissionPrefab progressMissonPrefab;
    // 미션 목록 UI 프리팹 저장 리스트
    private List<csProgressMissionPrefab> progressMissonList = new List<csProgressMissionPrefab>();





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
        //E_missonStatus = MissionStatus.MissonCreated;
        //_missonUIManager.ChangeMissonPanel(2);

    }

    // 미션 생성 취소
    public void CancleCreateMisson()
    {
        // 미션받아오기 전에 전에 취소 누르면 상태바꾸고 취소전환
        E_missonStatus = MissionStatus.None;
        csUI_Manager.Instance.PopupMission(false);
    }

    // 현재 미션목록을 기반으로 미션 목록 UI 생성
    public void SetCreatedMissonUI()
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
                        createdMissionList.Add(createdMission);
                    }
                }
            }
    }

    private void ClearCreatedMissionList()
    {
        foreach (var mission in createdMissionList)
        {
            if (mission != null)
                Destroy(mission.gameObject);
        }
        createdMissionList.Clear();
    }


    public void StartMission(int missionIndex)
    {
        missionContainer = aiCreatedMissions.missions[missionIndex];

        _missonUIManager.ChangeToProgressMission();

        currentMissionIndex = missionIndex;
    }

    public void SetProgressMissionUI()
    {
        ResetProgressMissionList();

        for (int i = 0; i < missionContainer.missionStepDetails.Count; ++i)
        {
            if (progressMissonPrefab)
            {
                csProgressMissionPrefab progressMission = Instantiate(progressMissonPrefab, progressMissonHolder, false);
                if (progressMission)
                {
                    progressMission.Init(i, missionContainer.missionStepDetails[i]);
                    progressMissonList.Add(progressMission);
                }
            }
        }
    }

    private void ResetProgressMissionList()
    {
        foreach (var progressMission in progressMissonList)
        {
            if (progressMission != null)
                Destroy(progressMission.gameObject);
        }
        progressMissonList.Clear();
    }

    //

    public void ClearCurrentProgressMission()
    {
        MissionStep currentMissionDto = aiCreatedMissions.missions[currentMissionIndex].missionStepDetails[currentMissionIndex];

        currentMissionDto.IsCleared = true;

        progressMissonList[currentMissionStepIndex].SetClearUI();

        currentMissionStepIndex++;

        // 미션스텝 디테일보다 크면 해당 미션 클리어
        if(currentMissionStepIndex >=  missionContainer.missionStepDetails.Count)
        {
            ClearCurrentCreatedMission();
        }
    }
    
    // 현재 진행중이던 미션 클리어
    private void ClearCurrentCreatedMission()
    {
        aiCreatedMissions.missions[currentMissionIndex].IsCleared = true;
        createdMissionList[currentMissionIndex].SetClearUI();
    }

    public MissionStep GetCurrentMissionDto()
    {
        return missionContainer.missionStepDetails[currentMissionStepIndex];
    }

    

}
