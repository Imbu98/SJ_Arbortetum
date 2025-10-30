using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csSearchManager : MonoBehaviour
{
    [SerializeField] private GameObject searchScreen; // 장소검색 시 띄울 화면

    [SerializeField] private GameObject searchPlaceObject; // 장소 검색을 위한 오브젝트
    [SerializeField] private GameObject pathFindObject; // 길찾기를 위한 오브젝트

    [HideInInspector] public string searchLocation = ""; // 현재 찾으려는 장소 문자열
    [HideInInspector] public string pathFind_StartLocation = ""; // 길찾기 출발지 장소 문자열
    [HideInInspector] public string pathFind_EndLocation = "";   // 길찾기 도착지 장소 문자열

    [SerializeField] private TMP_InputField search_InputField; 
    [SerializeField] private TMP_InputField pathFind_StartInputField; // 길찾기 출발 InputField
    [SerializeField] private TMP_InputField pathFind_EndInputField; // 길찾기 도착 InputField






}
