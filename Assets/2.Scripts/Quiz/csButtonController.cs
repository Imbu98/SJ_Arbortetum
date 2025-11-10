
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    // 버튼 선택 모드를 Inspector에서 정할 수 있도록 Enum(열거형)으로 정의
    public enum SelectionMode
    {
        Single,  // 하나만 선택 (라디오 버튼)
        Multiple // 여러 개 중복 선택 (체크박스)
    }

    [Header("설정")]
    public SelectionMode selectionMode = SelectionMode.Single; // 기본 모드는 '하나만 선택'

    [Header("버튼 이미지 (Sprite)")]
    public Sprite checkedSprite;   // 선택됐을 때의 이미지
    public Sprite uncheckedSprite; // 선택 해제됐을 때의 이미지
    public Sprite checkedBoxSprite; // 선택됐을 때의 박스이미지
    public Sprite uncheckedBoxSprite; // 선택 해제됐을 때의 박스이미지
    [Header("바꿀 버튼 이미지 ")]
    [SerializeField] private List<Image> buttonSprites;

    [Header("관리할 버튼 목록")]
    public List<Button> buttons;   
    // 각 버튼의 선택 상태를 저장하는 리스트 (true: 선택됨, false: 해제됨)
    private List<bool> buttonStates;

    private void OnEnable()
    {
        InitializeButtons();
    }
    private void OnDisable()
    {
        foreach(var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    // 버튼 초기 설정
    private void InitializeButtons()
    {
        buttonStates = new List<bool>();

        for (int i = 0; i < buttons.Count; i++)
        {
            // 모든 버튼의 초기 상태를 '선택 안 됨'으로 설정
            buttonStates.Add(false);

            // 각 버튼에 클릭 이벤트를 동적으로 추가
            // 클로저(closure)를 사용하여 각 버튼이 자신의 인덱스(i)를 기억
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }

        // 모든 버튼의 이미지를 초기 상태(unchecked)로 업데이트
        UpdateAllButtonVisuals();
    }

    // 버튼이 클릭됐을 때 호출될 함수
    public void OnButtonClick(int index)
    {
        if (selectionMode == SelectionMode.Single)
        {
            for (int i = 0; i < buttonStates.Count; i++)
            {
                buttonStates[i] = (i == index);
            }

            // Single 모드에서는 전체 버튼 비주얼 갱신 필요
            UpdateAllButtonVisuals();
        }
        else // selectionMode == SelectionMode.Multiple
        {
            // 상태 반전
            buttonStates[index] = !buttonStates[index];

            // Multiple 모드에서는 해당 인덱스만 갱신
            UpdateButtonVisual(index);
        }
    }

    // 특정 인덱스 버튼만 업데이트
    private void UpdateButtonVisual(int index)
    {
        if (buttonStates[index]) // 선택됨
        {
            buttonSprites[index].sprite = checkedSprite;
            buttons[index].GetComponent<Image>().sprite = checkedBoxSprite;
           
        }
        else // 해제됨
        {
            buttonSprites[index].sprite = uncheckedSprite;
            buttons[index].GetComponent<Image>().sprite = uncheckedBoxSprite;
        }
    }

    // 기존 전체 업데이트 함수 (Single 모드용)
    private void UpdateAllButtonVisuals()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            UpdateButtonVisual(i);
        }
    }

}