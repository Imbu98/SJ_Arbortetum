using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csProgressMissionPrefab : MonoBehaviour
{
    [SerializeField] private Button openMissionInfoButton;
    [SerializeField] private Image openInfoIcon_Img;
    [SerializeField] private Image closeInfoIcon_Img;
    [SerializeField] private GameObject MissionObject; // 미션 요약 리스트 
    [SerializeField] private GameObject MissionInfoObject; // 리스트를 누르면 나타나는 미션 정보 창
    [SerializeField] private TextMeshProUGUI missonDestination_TMP; // 목적지 이름
    [SerializeField] private TextMeshProUGUI missonSuccess_TMP; // 완료 여부
    [SerializeField] private TextMeshProUGUI infoDestination_TMP; // 미션 정보 창 목적지 이름
    [SerializeField] private TextMeshProUGUI infoDescription_TMP; // 미션 정보 창 거리
    [SerializeField] private TextMeshProUGUI infoMissonDistance_TMP; // 미션 정보 창 거리
    [SerializeField] private GameObject missionLockObject; // 미션 잠금 오브젝트

    [SerializeField] private Button missionObserveButton; // 미션 관찰하기 버튼
    [SerializeField] private Button missionPathFindButton; // 미션 길찾기 버튼
    [SerializeField] private Button missionForgiveButton; // 미션 포기하기 버튼



    public void Init(int missonIndex, MissionStep missionStep)
    {

    }

    public void SetClearUI()
    {

    }
}
