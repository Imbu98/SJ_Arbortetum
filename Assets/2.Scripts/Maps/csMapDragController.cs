using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class csMapDragController : MonoBehaviour , IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform mapParentReact;
    // 맵 이동 관련 변수 
    private RectTransform mapRect;
    
    private Vector2 mapSize;
    private Vector2 screenSize;

    private bool isDragging = false;
    private int dragFingerId = -1;
    private Vector2 lastDragPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 이미 드래그 중이라면 무시
        if (isDragging)
            return;

        // 드래그 시작 → 시작한 손가락 ID 저장
        isDragging = true;
        dragFingerId = eventData.pointerId;
        lastDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        // 드래그 중 기준이 되는 손가락이 아니면 무시
        if (eventData.pointerId != dragFingerId)
            return;

        // 기준 손가락이 이벤트를 보내는 동안은 Position 계산
        Vector2 currentPos = eventData.position;
        Vector2 delta = currentPos - lastDragPos;
        lastDragPos = currentPos;

        mapParentReact.anchoredPosition += delta;

        csMapManager.Instance.ClampMap();
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        // 기준 손가락이 드래그 종료되면 전체 종료
        if (eventData.pointerId == dragFingerId)
        {
            isDragging = false;
            dragFingerId = -1;
        }
    }

    
}
