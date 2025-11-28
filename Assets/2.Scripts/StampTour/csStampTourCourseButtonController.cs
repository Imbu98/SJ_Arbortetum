using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Data;

public class csStampTourCourseButtonController : MonoBehaviour
{
    // 스탬프 성공 여부 스프라이트
    [SerializeField] private List<Sprite> courseButtonSprite;

    [SerializeField] private LocationData locationData;

    [SerializeField] private Button courseButton;

    [SerializeField] private string courseDescriptionKey;

    private int courseIndex=-1;

    private void OnEnable()
    {
        courseButton.onClick.AddListener(OnCourseButtonClicked);
    }

    private void OnDisable()
    {
        courseButton.onClick.RemoveAllListeners();
    }

    public void SetCourseButtonUI(bool isCleared,int index)
    {
        courseIndex = index;

        if(isCleared)
        {
            courseButton.image.sprite = courseButtonSprite[1]; // 클리어된 스탬프 이미지로 변경
            courseButton.interactable = false; // 버튼 비활성화
        }
        else
        {
            courseButton.image.sprite = courseButtonSprite[0]; // 기본 스탬프 이미지로 변경
            courseButton.interactable = true; // 버튼 비활성화
        }
    }
    private void OnCourseButtonClicked()
    {
        csStampTourManager.Instance.currentTourLocationIndex = courseIndex;
        csStampTourManager.Instance.currentTourLocationData = locationData;

        csPopupPanel.Instance.PopupStampTourElements(courseDescriptionKey,locationData);
    }
}
