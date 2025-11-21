using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Data;

public class csMissonUIManager : MonoBehaviour
{
    // 미션창 UI 전환용 패널 리스트
    [SerializeField] private List<GameObject> missonPanels;

    private GameObject currentMissonPanel;

    public Button missionClearButton;

    private void OnEnable()
    {
        if(csMissionManager.Instance.IsMissonOnProgress)
        {
            ChangeToMissionStep();
        }
        else
        {
            // 아직 미션이 생성 안됐을 때
            if (csMissionManager.Instance.E_missonStatus == MissionStatus.None)
            {
                ChangeMissonPanel(0);
            }
            // 미션 생성 중일 때
            else if (csMissionManager.Instance.E_missonStatus == MissionStatus.MissionCreating)
            {
                ChangeMissonPanel(1);
            }
            // 미션이 생성 됐을 때 미션목록 보여주기
            else if (csMissionManager.Instance.E_missonStatus == MissionStatus.MissonCreated)
            {
                ChangeToMission();
            }
        }
        
    }

    // 미션관련 패널을 바꾸는 함수 ( 0:미션스타일고르기, 1:미션생성중 창, 2:미션 목록, 3:미션 진행)
    public void ChangeMissonPanel(int panelindex)
    {
        if (currentMissonPanel != null)
        {
            currentMissonPanel.SetActive(false);
        }

        currentMissonPanel = missonPanels[panelindex];

        currentMissonPanel.SetActive(true);
    }

    // 생성된 미션 목록 창으로 변경
    public void ChangeToMission()
    {
        ChangeMissonPanel(2);

        csMissionManager.Instance.SetMissonUI();
    }

    // 미션진행중 창으로 변경
    public void ChangeToMissionStep()
    {
        ChangeMissonPanel(3);

        csMissionManager.Instance.SetMissionStepUI();
    }

    // 미션성공연출 화면으로 변경
    public void ChangeToMissionClearPanel()
    {
        csUIManager.Instance.BlockBackButton(true);

        ChangeMissonPanel(4);
    }


}
