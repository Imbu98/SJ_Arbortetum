using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csCreatedMissonPrefab : MonoBehaviour
{
    [SerializeField] private GameObject clearObject; // 미션을 깼는지에 대한 표시 UI
    [SerializeField] private Image uiBackground_IMG; // 미션UI 배경
    [SerializeField] private TextMeshProUGUI createdMissonTitle_TMP; // 미션 제목
    [SerializeField] private TextMeshProUGUI createdMissonDescription_TMP; // 미션 설명
    [SerializeField] private TextMeshProUGUI createdMissonDistance_TMP; // 거리
    [SerializeField] private TextMeshProUGUI createdMissonTimeTaken_TMP; // 소요시간
    [SerializeField] private Button missionStart_BTN; // 미션 시작 버튼

    [SerializeField] private List<Color> colors;

    public void Init(int missonIndex, MissionContainer missionInfo)
    {
        uiBackground_IMG.color = colors[missonIndex];

        createdMissonTitle_TMP.text = missionInfo.missionTitle;
        createdMissonDescription_TMP.text = missionInfo.Description;
        createdMissonDistance_TMP.text = $"{missionInfo.missionDistance}m";
        createdMissonTimeTaken_TMP.text = missionInfo.missonTimeTaken.ToString(); // 추후 로컬라제이션 추가
        missionStart_BTN.onClick.AddListener(() => StartMission(missonIndex));
        clearObject.SetActive(missionInfo.IsCleared);
    }

    private void StartMission(int index)
    {
        csMissionManager.Instance.StartMission(index);
    }
}
