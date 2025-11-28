using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class csStampTourUIManager : MonoBehaviour
{
    // 미션창 UI 전환용 패널 리스트
    [SerializeField] private List<GameObject> stampTourPanels;

    private GameObject currentStampTourPanel;

    [Header("미션 선택 패널 관련")]
    [SerializeField] private List<Button> selectstampTourCourseButtons;
    [SerializeField] private Button stampTourStartButton;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Sprite unSelectedSprite;
    [SerializeField] private Color unSelectedTextColor;

    private int currentSelectedIndex = -1;

    [Header("미션 진행 패널 관련")]
    [SerializeField] public List<csStampTourBody> stampTourBodyList;

    [SerializeField] public Button resetCurrentCourseButton;

    // 다시 코스 선택으로 돌아올 수 있는 확인 버튼
    [SerializeField] public Button confirmButton;

    private void OnEnable()
    {
        // -1이 아니면 스탬프 투어 진행중
        if (csStampTourManager.Instance.currentStampTourIndex != -1)
        {
            OpenAndSetUI();
            
        }
        else
        {
            ChangeStampTourPanel(0);
            AddListenerToButtons();
        }
        confirmButton.onClick.AddListener(returnToSelectCourceScreen);
    }

    private void OnDisable()
    {
        // 리스너 해제
        foreach (var btn in selectstampTourCourseButtons)
        {
            btn.onClick.RemoveAllListeners();
        }

        stampTourStartButton.onClick.RemoveAllListeners();
        resetCurrentCourseButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
    }

    private void AddListenerToButtons()
    {
        // 미션 생성 버튼 비활성화
        StampTourStartButtonButtonInteractable(false);

        // 버튼 클릭 리스너 등록
        for (int i = 0; i < selectstampTourCourseButtons.Count; i++)
        {
            int index = i; // 클로저 문제 방지
            selectstampTourCourseButtons[i].onClick.AddListener(() => OnSelectButtonClicked(index));
            // UI초기화
            selectstampTourCourseButtons[i].image.sprite = unSelectedSprite;
            selectstampTourCourseButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = unSelectedTextColor;
            UpdateCourseClearUI();
        }

        stampTourStartButton.onClick.AddListener(OnClickstampTourStartButton);
        resetCurrentCourseButton.onClick.AddListener(PopupResetCurrentCourse);
    }

    private void UpdateCourseClearUI()
    {
        for (int i = 0; i < selectstampTourCourseButtons.Count; i++)
        {
            bool isCourseCleared = csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList[i].IsCleared;
            selectstampTourCourseButtons[i].GetComponent<csStampTourSelectButtonController>().SetClearObjectActive(isCourseCleared);
        }
    }


    // 미션관련 패널을 바꾸는 함수 ( 0: 스탬프 투어 미션고르기, 1:스탬프 투어 미션 화면)
    public void ChangeStampTourPanel(int panelindex)
    {
        if (currentStampTourPanel != null)
        {
            currentStampTourPanel.SetActive(false);
        }

        currentStampTourPanel = stampTourPanels[panelindex];

        currentStampTourPanel.SetActive(true);
    }

    private void OnSelectButtonClicked(int index)
    {
        // 같은 버튼 다시 누르면 유지
        if (currentSelectedIndex == index)
            return;

        currentSelectedIndex = index;

        UpDateButtonUI(index);

        StampTourStartButtonButtonInteractable(true);
    }


    private void UpDateButtonUI(int index)
    {
        for (int i = 0; i < selectstampTourCourseButtons.Count; i++)
        {
            if (i == index)
            {
                selectstampTourCourseButtons[i].image.sprite = selectedSprite;
                selectstampTourCourseButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = selectedTextColor;
            }
            else
            {
                selectstampTourCourseButtons[i].image.sprite = unSelectedSprite;
                selectstampTourCourseButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = unSelectedTextColor;
            }
        }

    }

    private void StampTourStartButtonButtonInteractable(bool IsInteractable)
    {
        stampTourStartButton.interactable = IsInteractable;

        csStampTourManager.Instance.currentStampTourIndex = currentSelectedIndex;
    }

    private void OpenAndSetUI()
    {
        SetMissionBody();

        ChangeStampTourPanel(1);
    }

    private void OnClickstampTourStartButton()
    {
        OpenAndSetUI();
    }

    public void SetMissionBody()
    {
        foreach (var body in stampTourBodyList)
        {
            body.gameObject.SetActive(false);
        }

        for (int i = 0; i < stampTourBodyList.Count; i++)
        {
            if (i == csStampTourManager.Instance.currentStampTourIndex)
            {
                stampTourBodyList[i].gameObject.SetActive(true);
            }
            else
            {
                stampTourBodyList[i].gameObject.SetActive(false);
            }
        }
    }

    public void PopupResetCurrentCourse()
    {
        csPopupPanel.Instance.PopupResetCurrentCourse(ResetCurrentCourse);

    }

    private void ResetCurrentCourse()
    {
        int currentStampTourCourseIndex = csStampTourManager.Instance.currentStampTourIndex;

        foreach(var location in csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList[currentStampTourCourseIndex].stampTourCourseList)
        {
            location.IsCleared = false;
        }

        csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList[currentStampTourCourseIndex].IsCleared = false;

        csStampTourManager.Instance.currentStampTourIndex = -1;

        currentSelectedIndex = -1;

        csUIManager.Instance.PopupStampTour(false);

        csSaveLodeManager.Instance.SaveStampTour();
    }

    private void returnToSelectCourceScreen()
    {
        csStampTourManager.Instance.currentStampTourIndex = -1;

        currentSelectedIndex = -1;

        ChangeStampTourPanel(0);

        AddListenerToButtons();

        csSaveLodeManager.Instance.SaveStampTour();
    }
    //private void PopupResetAllCourse()
    //{
    //    csPopupPanel.Instance.PopupResetAllCourses(ResetAllCourses);
    //}

    //private void ResetAllCourses()
    //{
    //    for(int i=0; i< csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList.Count; i++)
    //    {
    //        foreach (var course in csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList)
    //        {
    //            course.IsCleared = false;
    //        }

    //        foreach (var location in csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList[i].stampTourCourseList)
    //        {
    //            location.IsCleared = false;
    //        }
    //    }
    //    csStampTourManager.Instance.currentStampTourProgressData.stampTourIndex = -1;
    //    csUIManager.Instance.PopupStampTour(false);
    //    csSaveLodeManager.Instance.SaveStampTour();
    //}
}


