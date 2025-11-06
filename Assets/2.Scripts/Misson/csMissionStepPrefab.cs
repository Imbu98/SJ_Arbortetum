using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csMissionStepPrefab : MonoBehaviour
{
    [SerializeField] private Button openMissionInfoButton; // 미션 정보 여는 버튼
    [SerializeField] private Image openInfoIcon_Img;
    [SerializeField] private Image closeInfoIcon_Img;
    [SerializeField] private GameObject missionObject; // 미션 요약 리스트 
    [SerializeField] private GameObject missionInfoObject; // 리스트를 누르면 나타나는 미션 정보 창
    [SerializeField] private TextMeshProUGUI missionDestination_TMP; // 목적지 이름
    [SerializeField] private TextMeshProUGUI infoDestination_TMP; // 미션 정보 창 목적지 이름
    [SerializeField] private TextMeshProUGUI infoDescription_TMP; // 미션 정보 창 거리
    [SerializeField] private TextMeshProUGUI infoMissonDistance_TMP; // 미션 정보 창 거리
    [SerializeField] private GameObject missionLockObject; // 미션 잠금 오브젝트

    [SerializeField] private Button missionObserveButton; // 미션 관찰하기 버튼
    [SerializeField] private Button missionPathFindButton; // 미션 길찾기 버튼
    [SerializeField] private Button clearTestButton;

    private bool IsMissionInfoOpened;

    public void Init(int missionIndex, MissionStep missionStep)
    {
        int currentStepIndex = csMissionManager.Instance.currentMissionStepIndex;
        bool isCleared = missionStep.IsCleared;
        bool isCurrentStep = (missionIndex == currentStepIndex);

        missionDestination_TMP.text = missionStep.destinationName;
        infoDestination_TMP.text = missionStep.destinationName;
        infoDescription_TMP.text = missionStep.Description;
        infoMissonDistance_TMP.text = missionStep.missionDistance + "m";

        openMissionInfoButton.onClick.AddListener(OpenMissionInfo);

        // ✅ UI 활성화 규칙 적용
        if (isCleared)
        {
            // 클리어한 스텝
            missionInfoObject.SetActive(false);
            missionObject.SetActive(true);
            missionLockObject.SetActive(false);
        }
        else
        {
            if (isCurrentStep)
            {
                // 진행 중인 스텝
                missionInfoObject.SetActive(true);
                missionObject.SetActive(false);
                missionLockObject.SetActive(true);
            }
            else
            {
                // 아직 클리어 안 했고, 현재 스텝도 아님
                missionInfoObject.SetActive(false);
                missionObject.SetActive(true);
                missionLockObject.SetActive(true);
            }
        }

        IsMissionInfoOpened = missionInfoObject.activeSelf;

        missionObserveButton.onClick.RemoveAllListeners();
        missionObserveButton.onClick.AddListener(() =>
        {
            csImageManager.Instance.SetCameraScreen();
        });

        missionPathFindButton.onClick.RemoveAllListeners();
        missionPathFindButton.onClick.AddListener(() =>
        {
            csUI_Manager.Instance.PopupMap(true);

            csMapManager.Instance._searchManager.ClearPathFindUI();

            // 도착지점에 미션 데이터 경도,위도를 넣어서 길찾기 시작
            csMapManager.Instance._searchManager.SetSearchUI(
     new LocationData(
         missionStep.destinationName,
         missionStep.destinationName,
         missionStep.destinationCoordinate.Latitude,
         missionStep.destinationCoordinate.Longitude,
         -2
     ),
     2
 );

        });

        clearTestButton.onClick.RemoveAllListeners();
        clearTestButton.onClick.AddListener(()=>csMissionManager.Instance.ClearCurrentMissionStep());
    }

    private void OpenMissionInfo()
    {
        if(IsMissionInfoOpened)
        {
            missionInfoObject.SetActive(false);
            IsMissionInfoOpened=false;
        }
        else
        {
            missionInfoObject.SetActive(true);
            IsMissionInfoOpened=true;
        }
        // 열려있는게 열림 아이콘 활성화
        openInfoIcon_Img.gameObject.SetActive(!IsMissionInfoOpened);
        // 열려있으면 닫힘 아이콘 활성화
        closeInfoIcon_Img.gameObject .SetActive(IsMissionInfoOpened);
    }

    public void SetProgressUI()
    {
        missionLockObject.SetActive(false);
        missionInfoObject.SetActive(true);
        missionObject.SetActive(false);

        IsMissionInfoOpened = true;
    }

    public void SetClearUI()
    {
        // 클리어 UI로 변경
        missionLockObject.SetActive(false);
        missionInfoObject.SetActive(false);
        missionObject.SetActive(true);

        // 완료 후 해당미션은 관찰하기와 길찾기 버튼 비활성화
        missionObserveButton.interactable = false;
        missionPathFindButton.interactable = false; 

        IsMissionInfoOpened = false;
    }
}
