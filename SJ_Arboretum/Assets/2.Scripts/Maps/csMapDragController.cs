using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class csMapDragController : MonoBehaviour , IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RawImage mapRawImage;
    // 맵 이동 관련 변수 
    private RectTransform mapRect;
    private Vector2 lastDragPosition;
    private Vector2 mapSize;
    private Vector2 screenSize;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        mapRect = mapRawImage.rectTransform;
        mapSize = mapRect.sizeDelta;
        screenSize = new Vector2(Screen.width, Screen.height);
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mapRect == null)
            return;

        // 드래그 이동 계산
        Vector2 delta = eventData.position - lastDragPosition;
        lastDragPosition = eventData.position;

        // 위치 업데이트
        mapRect.anchoredPosition += delta;

        // 경계 제한 적용
        ClampMapPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 필요 시 드래그 끝났을 때 로직 추가
    }

    /// <summary>
    /// 맵이 화면을 벗어나지 않도록 위치 제한
    /// </summary>
    private void ClampMapPosition()
    {
        Vector2 pos = mapRect.anchoredPosition;

        float halfMapWidth = mapSize.x / 2f;
        float halfMapHeight = mapSize.y / 2f;
        float halfScreenWidth = screenSize.x / 2f;
        float halfScreenHeight = screenSize.y / 2f;

        // 맵이 화면보다 클 경우에만 경계 제한 적용
        if (mapSize.x > screenSize.x)
        {
            float maxX = halfMapWidth - halfScreenWidth;
            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        }
        else
        {
            pos.x = 0; // 화면보다 작으면 가운데 고정
        }

        if (mapSize.y > screenSize.y)
        {
            float maxY = halfMapHeight - halfScreenHeight;
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
        }
        else
        {
            pos.y = 0;
        }

        mapRect.anchoredPosition = pos;
    }
}
