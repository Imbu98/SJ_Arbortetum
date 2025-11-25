using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csSelectMissonStyle : MonoBehaviour
{
    [SerializeField] private List<Button> selectMissonStyleButtons;

    [SerializeField] private Button missonCreateButton;

    private int currentIndex = -1; // 현재 선택된 버튼 인덱스
    [Header("Button Colors")]
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Sprite unSelectedSprite;
    [SerializeField] private Color unSelectedTextColor;

    private void OnEnable()
    {
        // 미션 생성 버튼 비활성화
        MissonCreateButtonInteractable(false);

        // 버튼 클릭 리스너 등록
        for (int i = 0; i < selectMissonStyleButtons.Count; i++)
        {
            int index = i; // 클로저 문제 방지
            selectMissonStyleButtons[i].onClick.AddListener(() => OnSelectButtonClicked(index));
            // UI초기화
            selectMissonStyleButtons[i].image.sprite = unSelectedSprite;
            selectMissonStyleButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = unSelectedTextColor;

        }

        //missonCreateButton.onClick.AddListener(OnClickMissonCreateButton);
    }

    private void OnDisable()
    {
        // 리스너 해제
        foreach (var btn in selectMissonStyleButtons)
        {
            btn.onClick.RemoveAllListeners();
        }

        missonCreateButton.onClick.RemoveAllListeners();
    }

    private void OnSelectButtonClicked(int index)
    {
        // 같은 버튼 다시 누르면 유지
        if (currentIndex == index)
            return;

        currentIndex = index;

        UpDateButtonUI(index);

        MissonCreateButtonInteractable(true);
    }

    private void UpDateButtonUI(int index)
    {
        for (int i = 0; i < selectMissonStyleButtons.Count; i++)
        {
            if(i==index)
            {
                selectMissonStyleButtons[i].image.sprite = selectedSprite;
                selectMissonStyleButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = selectedTextColor;
            }
            else
            {
                selectMissonStyleButtons[i].image.sprite = unSelectedSprite;
                selectMissonStyleButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = unSelectedTextColor;
            }
        }

    }

    private void MissonCreateButtonInteractable(bool IsInteractable)
    {
        missonCreateButton.interactable = IsInteractable;
    }

    //private void OnClickMissonCreateButton()
    //{
    //    csMissionManager.Instance.CreateMisson();
    //}
}
