using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csCanvasController : MonoBehaviour
{
     private float tabletAspectRatioThreshold = 1.73f;

    [SerializeField] private CanvasScaler canvasScaler;
    void Awake()
    {
        AdjustCanvasScaler();
    }
    void AdjustCanvasScaler()
    {
        // 현재 화면의 가로, 세로 길이를 가져옵니다.
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 현재 화면의 종횡비를 계산합니다. (세로가 긴지, 가로가 긴지 상관없이 계산)
        // 예: 1080x1920 (세로) -> 1.77, 1920x1080 (가로) -> 1.77
        float currentAspectRatio = Mathf.Max(screenWidth, screenHeight) / Mathf.Min(screenWidth, screenHeight);

        Debug.Log($"현재 화면 해상도: {screenWidth}x{screenHeight}, 종횡비: {currentAspectRatio}");

        if (currentAspectRatio > tabletAspectRatioThreshold)
        {
            // 스마트폰과 같이 세로로 긴 기기일 경우
            Debug.Log("스마트폰 UI 모드: 너비(Width)를 기준으로 스케일링합니다. (Match = 0)");
            
            canvasScaler.matchWidthOrHeight = 0; // 너비에 맞춤
        }
        else
        {
            // 태블릿과 같이 넓은 기기일 경우
            Debug.Log("태블릿 UI 모드: 높이(Height)를 기준으로 스케일링합니다. (Match = 1)");

            canvasScaler.matchWidthOrHeight = 1; // 높이에 맞춤
        }
    }
}