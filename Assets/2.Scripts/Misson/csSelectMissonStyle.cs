using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csSelectMissonStyle : MonoBehaviour
{
    [SerializeField] private List<Button> selectMissonStyleButtons;

    [SerializeField] private Button missonCreateButton;

    private int currentIndex = -1; // 현재 선택된 버튼 인덱스
    [Header("Button Colors")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;

    private void OnEnable()
    {
        // 미션 생성 버튼 비활성화
        MissonCreateButtonInteractable(false);

        // 버튼 클릭 리스너 등록
        for (int i = 0; i < selectMissonStyleButtons.Count; i++)
        {
            int index = i; // 클로저 문제 방지
            selectMissonStyleButtons[i].onClick.AddListener(() => OnSelectButtonClicked(index));
        }

        missonCreateButton.onClick.AddListener(() => csMissionManager.Instance.CreateMisson());
    }

    private void OnDisable()
    {
        // 리스너 해제
        foreach (var btn in selectMissonStyleButtons)
        {
            btn.onClick.RemoveAllListeners();
            btn.image.color = normalColor;
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
                selectMissonStyleButtons[i].image.color = selectedColor;
            }
            else
            {
                selectMissonStyleButtons[i].image.color = normalColor;
            }
        }

    }

    private void MissonCreateButtonInteractable(bool IsInteractable)
    {
        missonCreateButton.interactable = IsInteractable;

        if (IsInteractable)
        {
            missonCreateButton.image.color = selectedColor;
        }
        else
        {
            missonCreateButton.image.color = normalColor;
        }
    }
}
