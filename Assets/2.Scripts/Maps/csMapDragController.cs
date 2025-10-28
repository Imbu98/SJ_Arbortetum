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
        
        
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mapRawImage == null)
            return;

        // 드래그 이동 계산
        Vector2 delta = eventData.position - lastDragPosition;
        lastDragPosition = eventData.position;

        // 위치 업데이트
        mapRawImage.rectTransform.anchoredPosition += delta;

        // 경계 제한 적용
        csMapManager.Instance.ClampMapPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        // 필요 시 드래그 끝났을 때 로직 추가
    }

    
}
